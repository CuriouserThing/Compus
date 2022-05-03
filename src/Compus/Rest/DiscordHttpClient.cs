using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Compus.Caching;
using Compus.Json;
using Microsoft.Extensions.Logging;

namespace Compus.Rest;

public class DiscordHttpClient : IDiscordHttpClient
{
    private const string Scheme = "https";
    private const string Host = "discord.com";
    private const string BasePath = "/api/v";
    private const int ApiVersion = 9;
    private const string AuthenticationScheme = "Bot";

    // This is a soft limit (only balances memory usage and GC spikes) and a per-bucket limit.
    // Ultimately, this number shouldn't matter too much.
    private const int ResourceConcurrencyLimit = 100;

    // In general, keep buckets alive for a week.
    // Since interaction tokens are only valid for 15 minutes, keep their buckets alive for only an hour.
    private static readonly TimeSpan ChannelBucketLifespan = TimeSpan.FromDays(7);
    private static readonly TimeSpan GuildBucketLifespan = TimeSpan.FromDays(7);
    private static readonly TimeSpan WebhookBucketLifespan = TimeSpan.FromDays(7);
    private static readonly TimeSpan InteractionBucketLifespan = TimeSpan.FromHours(1);

    private readonly ResourceLimiter<Snowflake> _channelLimiter =
        new(ResourceConcurrencyLimit, new LifespanEvictionPolicy(ChannelBucketLifespan));

    private readonly ResourceLimiter<Snowflake> _guildLimiter =
        new(ResourceConcurrencyLimit, new LifespanEvictionPolicy(GuildBucketLifespan));

    private readonly ResourceLimiter<Snowflake> _webhookLimiter =
        new(ResourceConcurrencyLimit, new LifespanEvictionPolicy(WebhookBucketLifespan));

    private readonly ResourceLimiter<string> _interactionLimiter =
        new(ResourceConcurrencyLimit, new LifespanEvictionPolicy(InteractionBucketLifespan));

    private readonly ResourceLimiter<Unit> _globalLimiter =
        new(ResourceConcurrencyLimit, new NoEvictionPolicy());

    private readonly SemaphoreSlim _bucketlessRequestLock = new(1, 1);
    private readonly Dictionary<Endpoint, string> _endpointBuckets = new();
    private readonly HttpClient _client = new();
    private readonly string _token;
    private readonly ILogger _logger;
    private readonly Stopwatch _stopwatch;
    private readonly DateTimeOffset _stopwatchEpoch;

    public DiscordHttpClient(Token token, ILogger<DiscordHttpClient> logger)
    {
        _token = token;
        _logger = logger;

        _stopwatch = Stopwatch.StartNew();
        _stopwatchEpoch = DateTimeOffset.UtcNow;
    }

    public async Task<HttpResponseMessage> Send(DiscordHttpRequest request, CancellationToken cancellationToken)
    {
        var endpoint = new Endpoint(request.Method, request.Url);
        string? bucket;
        lock (_endpointBuckets)
        {
            _ = _endpointBuckets.TryGetValue(endpoint, out bucket);
        }

        // This layer of abstraction is mostly to gracefully allow buckets to populate at app start.
        // If no bucket is found, wait for other bucketless requests to possibly update buckets, then try again.
        if (bucket is null)
        {
            await _bucketlessRequestLock.WaitAsync(cancellationToken);
            try
            {
                lock (_endpointBuckets)
                {
                    _ = _endpointBuckets.TryGetValue(endpoint, out bucket);
                }

                if (bucket is null)
                {
                    return await SendBucketed(request, string.Empty, cancellationToken);
                }
            }
            finally
            {
                _bucketlessRequestLock.Release();
            }
        }

        return await SendBucketed(request, bucket, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendBucketed(DiscordHttpRequest request, string bucket, CancellationToken cancellationToken)
    {
        async Task<Response> SendFunc<T>(ResourceLimiter<T> limiter, T scope)
        {
            using (await limiter.Lock(scope, bucket))
            {
                return await SendUnlimited(request, bucket, cancellationToken);
            }
        }

        (HttpResponseMessage response, RateLimitHeaders limitHeaders, RateLimitContent? limitContent) =
            await ScopedAction(request.Scope, SendFunc, SendFunc, SendFunc);
        UpdateBuckets(request, limitHeaders);

        HttpStatusCode status = response.StatusCode;
        if (response.IsSuccessStatusCode)
        {
            LogRequestResponse(request, status, limitHeaders, limitContent);
            return response;
        }
        else if (status == HttpStatusCode.TooManyRequests)
        {
            LogRequestResponse(request, status, limitHeaders, limitContent);
            response.Dispose();
            throw new DiscordApiException("Rate limit unhandled.", status);
        }
        else
        {
            ErrorContent? errorContent = null;
            try
            {
                await using Stream responseContent = await response.Content.ReadAsStreamAsync(cancellationToken);
                errorContent = await JsonSerializer.DeserializeAsync<ErrorContent>(responseContent, JsonOptions.SerializerOptions, cancellationToken);
            }
            catch (JsonException ex)
            {
                var statusNumber = (int)status;
                _logger.LogWarning(ex, "Discord returned the HTTP code {statusNumber} {status}, but we couldn't deserialize error content from the response.",
                                   statusNumber, status);
            }

            LogRequestResponse(request, status, limitHeaders, limitContent, errorContent);
            response.Dispose();
            if (errorContent is null)
            {
                throw new DiscordApiException("Unknown Discord API error encountered.", status);
            }
            else
            {
                throw new DiscordApiException(errorContent.Message, status)
                {
                    Code = errorContent.Code,
                    Errors = errorContent.Errors,
                };
            }
        }
    }

    private async Task<Response> SendUnlimited(DiscordHttpRequest request, string bucket, CancellationToken cancellationToken)
    {
        while (true)
        {
            long retry = GetRetry(request, bucket);
            long now = _stopwatch.ElapsedTicks;
            long retryAfter = retry - now;
            if (retryAfter > 0)
            {
                await Task.Delay(TimeSpan.FromTicks(retryAfter), cancellationToken);
            }

            string path = request.GetPath();
            var req = new HttpRequestMessage
            {
                Method = request.Method,
                RequestUri = new Uri($"{Scheme}://{Host}{BasePath}{ApiVersion}{path}"),
                Content = request.Content,
            };
            req.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationScheme, _token);
            HttpResponseMessage response = await _client.SendAsync(req, cancellationToken);
            HttpStatusCode status = response.StatusCode;

            var headers = RateLimitHeaders.ReadFromResponse(response, _logger);
            RateLimitContent? rateLimitContent = null;
            if (status == HttpStatusCode.TooManyRequests)
            {
                try
                {
                    await using Stream responseContent = await response.Content.ReadAsStreamAsync(cancellationToken);
                    rateLimitContent = await JsonSerializer.DeserializeAsync<RateLimitContent>(responseContent, JsonOptions.SerializerOptions, cancellationToken);
                }
                catch (JsonException ex)
                {
                    _logger.LogInformation(ex, "Couldn't deserialize rate limit response content. Ignoring content.");
                }
            }

            SetRetry(request, headers, rateLimitContent);
            if (status == HttpStatusCode.TooManyRequests)
            {
                LogRequestResponse(request, status, headers, rateLimitContent);
                response.Dispose();
                continue;
            }

            return new Response(response, headers, rateLimitContent);
        }
    }

    private long GetRetry(DiscordHttpRequest request, string bucket)
    {
        long GetFunc<T>(ResourceLimiter<T> limiter, T scope)
        {
            return limiter.GetRetry(scope, bucket);
        }

        long scopedRetry = ScopedAction(request.Scope, GetFunc, GetFunc, GetFunc);
        long globalRetry = _globalLimiter.GetRetry(Unit.Default, bucket);
        return Math.Max(scopedRetry, globalRetry);
    }

    private void SetRetry(DiscordHttpRequest request, RateLimitHeaders headers, RateLimitContent? content)
    {
        // First, determine the retry time.
        long now = _stopwatch.ElapsedTicks;
        long retry;
        if (content is not null && content.RetryAfter.IsSome(out float retryAfter))
        {
            retry = now + (long)(retryAfter * TimeSpan.TicksPerSecond);
        }
        else if (headers.Remaining.IsSome(out int remaining) && remaining < 1)
        {
            if (headers.ResetAfter.IsSome(out double resetAfter))
            {
                retry = now + (long)(resetAfter * TimeSpan.TicksPerSecond);
            }
            else if (headers.Reset.IsSome(out double reset))
            {
                // Fallback on absolute server time if needed (not ideal).
                TimeSpan retrySpan = DateTimeOffset.UnixEpoch + TimeSpan.FromSeconds(reset) - _stopwatchEpoch;
                retry = retrySpan.Ticks;
            }
            else { return; }
        }
        else { return; }

        // Then, cache the retry time accordingly.
        if (headers.Global.IsSome(out bool global) && global)
        {
            _globalLimiter.SetRetry(Unit.Default, string.Empty, false, retry);
        }
        else if (headers.Bucket.IsSome(out string? bucket))
        {
            Unit SetFunc<T>(ResourceLimiter<T> limiter, T scope)
            {
                bool shared = headers.Scope.IsSome(out string? limitScope) && limitScope == "shared";
                limiter.SetRetry(scope, bucket, shared, retry);
                return Unit.Default;
            }

            ScopedAction(request.Scope, SetFunc, SetFunc, SetFunc);
        }
        else
        {
            _logger.LogWarning("Rate limit headers and content are neither global nor bucketed. Ignoring.");
        }
    }

    private void UpdateBuckets(DiscordHttpRequest request, RateLimitHeaders limitHeaders)
    {
        if (!limitHeaders.Bucket.IsSome(out string? newBucket))
        {
            return;
        }

        HttpMethod method = request.Method;
        string url = request.Url;
        var endpoint = new Endpoint(method, url);
        string? oldBucket;
        lock (_endpointBuckets)
        {
            if (_endpointBuckets.TryGetValue(endpoint, out oldBucket) && oldBucket == newBucket)
            {
                return;
            }

            _endpointBuckets[endpoint] = newBucket;
        }

        if (oldBucket is not null)
        {
            _logger.LogInformation("Removed endpoint {method} {url} from bucket {oldBucket}.",
                                   method, url, oldBucket);
        }

        _logger.LogInformation("Added endpoint {method} {url} to bucket {newBucket}.",
                               method, url, newBucket);
    }

    private void LogRequestResponse(DiscordHttpRequest request,             HttpStatusCode status, RateLimitHeaders limitHeaders,
                                    RateLimitContent?  limitContent = null, ErrorContent?  errorContent = null)
    {
        HttpMethod method = request.Method;
        string path = request.GetPath();
        var statusNumber = (int)status;
        string? statusName = Enum.GetName(status);

        if (limitContent is not null && limitContent.RetryAfter.IsSome(out float retryAfter))
        {
            string bucketName;
            if (limitHeaders.Global.IsSome(out bool global) && global)
            {
                bucketName = "global";
            }
            else if (limitHeaders.Bucket.IsSome(out string? bucket))
            {
                bucketName = bucket;
            }
            else
            {
                bucketName = "unknown";
            }

            var time = TimeSpan.FromSeconds(retryAfter);
            _logger.LogInformation(
                "{method} {path} -> {statusNumber} {statusName}. Rate limited at bucket {bucketName}; retry after {time}.",
                method, path, statusNumber, statusName, bucketName, time);
        }
        else if (limitHeaders.Remaining.IsSome(out int remaining) &&
            limitHeaders.Limit.IsSome(out int limit) &&
            limitHeaders.ResetAfter.IsSome(out double resetAfter))
        {
            var time = TimeSpan.FromSeconds(resetAfter);
            _logger.LogDebug(
                "{method} {path} -> {statusNumber} {statusName}. {remaining}/{limit} remaining; resets after {time}.",
                method, path, statusNumber, statusName, remaining, limit, time);
        }

        if (errorContent is null) { return; }

        if (errorContent.Errors.IsSome(out IReadOnlyList<DataError>? errors))
        {
            foreach (DataError error in errors)
            {
                string code = error.Code;
                string message = error.Message;
                if (error.Path.IsSome(out string? errorPath))
                {
                    _logger.LogWarning(
                        "{method} {path} -> Error {code}: {message}",
                        method, path, code, message);
                }
                else
                {
                    _logger.LogWarning(
                        "{method} {path} -> Error {code} in {errorPath}: {message}",
                        method, path, code, errorPath, message);
                }
            }
        }
        else
        {
            ErrorCode code = errorContent.Code;
            string message = errorContent.Message;
            _logger.LogWarning(
                "{method} {path} -> Error {code}: {message}",
                method, path, code, message);
        }
    }

    private T ScopedAction<T>(ResourceScope                                  scope,
                              Func<ResourceLimiter<Snowflake>, Snowflake, T> snowflakeFunc,
                              Func<ResourceLimiter<string>, string, T>       stringFunc,
                              Func<ResourceLimiter<Unit>, Unit, T>           unitFunc)
    {
        Task<T> task = ScopedAction(scope,
                                    (limiter, s) => Task.FromResult(snowflakeFunc(limiter, s)),
                                    (limiter, s) => Task.FromResult(stringFunc(limiter, s)),
                                    (limiter, s) => Task.FromResult(unitFunc(limiter, s)));
        return task.Result;
    }

    private async Task<T> ScopedAction<T>(ResourceScope                                        scope,
                                          Func<ResourceLimiter<Snowflake>, Snowflake, Task<T>> snowflakeFunc,
                                          Func<ResourceLimiter<string>, string, Task<T>>       stringFunc,
                                          Func<ResourceLimiter<Unit>, Unit, Task<T>>           unitFunc)
    {
        if (scope.Channel.IsSome(out Snowflake channel))
        {
            return await snowflakeFunc(_channelLimiter, channel);
        }
        else if (scope.InteractionToken.IsSome(out string? interactionToken))
        {
            return await stringFunc(_interactionLimiter, interactionToken);
        }
        else if (scope.Webhook.IsSome(out Snowflake webhook))
        {
            return await snowflakeFunc(_webhookLimiter, webhook);
        }
        else if (scope.Guild.IsSome(out Snowflake guild))
        {
            return await snowflakeFunc(_guildLimiter, guild);
        }
        else
        {
            return await unitFunc(_globalLimiter, Unit.Default);
        }
    }

    private record struct Endpoint(HttpMethod Method, string Url);

    private record Response(HttpResponseMessage Message, RateLimitHeaders LimitHeaders, RateLimitContent? LimitContent);

    #region IDisposable

    private bool _isDisposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) { return; }

        if (disposing)
        {
            _client.Dispose();
        }

        _isDisposed = true;
    }

    #endregion
}
