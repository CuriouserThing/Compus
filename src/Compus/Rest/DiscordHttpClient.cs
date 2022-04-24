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

    // In general, keep buckets alive for a week.
    // Since interaction tokens are only valid for 15 minutes, keep their buckets alive for only an hour.
    private static readonly TimeSpan ChannelBucketLifespan = TimeSpan.FromDays(7);
    private static readonly TimeSpan GuildBucketLifespan = TimeSpan.FromDays(7);
    private static readonly TimeSpan WebhookBucketLifespan = TimeSpan.FromDays(7);
    private static readonly TimeSpan InteractionBucketLifespan = TimeSpan.FromHours(1);

    private readonly RetryCache<Snowflake> _channelCache = new(new LifespanEvictionPolicy(ChannelBucketLifespan));
    private readonly RetryCache<Snowflake> _guildCache = new(new LifespanEvictionPolicy(GuildBucketLifespan));
    private readonly RetryCache<Snowflake> _webhookCache = new(new LifespanEvictionPolicy(WebhookBucketLifespan));
    private readonly RetryCache<string> _interactionCache = new(new LifespanEvictionPolicy(InteractionBucketLifespan));
    private readonly RetryCache<Unit> _globalCache = new(new NoEvictionPolicy());

    private readonly Dictionary<Endpoint, string> _endpointBuckets = new();
    private readonly Stopwatch _stopwatch;
    private readonly DateTimeOffset _stopwatchEpoch;
    private readonly HttpClient _client = new();
    private readonly ILogger _logger;
    private readonly string _token;

    public DiscordHttpClient(Token token, ILogger<DiscordHttpClient> logger)
    {
        _token = token;
        _logger = logger;

        _stopwatch = Stopwatch.StartNew();
        _stopwatchEpoch = DateTimeOffset.UtcNow;
    }

    public async Task<HttpResponseMessage> Send(DiscordHttpRequest request, CancellationToken cancellationToken)
    {
        string path = request.GetPath();
        var req = new HttpRequestMessage
        {
            Method = request.Method,
            RequestUri = new Uri($"{Scheme}://{Host}{BasePath}{ApiVersion}{path}"),
            Content = request.Content,
        };
        req.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationScheme, _token);

        while (true)
        {
            long retry = GetRetry(request);
            long now = _stopwatch.ElapsedTicks;
            long retryAfter = retry - now;
            if (retryAfter > 0)
            {
                await Task.Delay(TimeSpan.FromTicks(retryAfter), cancellationToken);
                continue;
            }

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
            UpdateBuckets(request, headers);

            if (response.IsSuccessStatusCode)
            {
                LogRequestResponse(request, status, headers, rateLimitContent);
                return response;
            }
            else if (status == HttpStatusCode.TooManyRequests)
            {
                LogRequestResponse(request, status, headers, rateLimitContent);
                response.Dispose();
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
                    _logger.LogWarning(ex, "Discord returned the HTTP code {status}, but we couldn't deserialize error content from the response.", status);
                }

                LogRequestResponse(request, status, headers, rateLimitContent, errorContent);
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
    }

    private long GetRetry(DiscordHttpRequest request)
    {
        var endpoint = new Endpoint(request.Method, request.Url);
        string? bucket;
        lock (_endpointBuckets)
        {
            _ = _endpointBuckets.TryGetValue(endpoint, out bucket);
        }

        ResourceScope scope = request.Scope;
        long scopedRetry = 0;
        if (scope.Channel.IsSome(out Snowflake channel))
        {
            scopedRetry = _channelCache.GetRetry(channel, bucket);
        }
        else if (scope.InteractionToken.IsSome(out string? interactionToken))
        {
            scopedRetry = _interactionCache.GetRetry(interactionToken, bucket);
        }
        else if (scope.Webhook.IsSome(out Snowflake webhook))
        {
            scopedRetry = _webhookCache.GetRetry(webhook, bucket);
        }
        else if (scope.Guild.IsSome(out Snowflake guild))
        {
            scopedRetry = _guildCache.GetRetry(guild, bucket);
        }

        long globalRetry = _globalCache.GetRetry(Unit.Default, bucket);
        return Math.Max(scopedRetry, globalRetry);
    }

    private void SetRetry(DiscordHttpRequest request, RateLimitHeaders headers, RateLimitContent? content)
    {
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

        if (headers.Global.IsSome(out bool global) && global)
        {
            // Global rate limit on the user
            _globalCache.SetRetry(Unit.Default, retry);
        }
        else if (headers.Scope.IsSome(out string? limitScope) && limitScope == "shared")
        {
            // Shared rate limit on the resource
            ResourceScope scope = request.Scope;
            if (scope.Channel.IsSome(out Snowflake channel))
            {
                _channelCache.SetRetry(channel, retry);
            }
            else if (scope.InteractionToken.IsSome(out string? interactionToken))
            {
                _interactionCache.SetRetry(interactionToken, retry);
            }
            else if (scope.Webhook.IsSome(out Snowflake webhook))
            {
                _webhookCache.SetRetry(webhook, retry);
            }
            else if (scope.Guild.IsSome(out Snowflake guild))
            {
                _guildCache.SetRetry(guild, retry);
            }
            else
            {
                _globalCache.SetRetry(Unit.Default, retry);
            }
        }
        else if (headers.Bucket.IsSome(out string? bucket))
        {
            ResourceScope scope = request.Scope;
            if (scope.Channel.IsSome(out Snowflake channel))
            {
                _channelCache.SetRetry(channel, bucket, retry);
            }
            else if (scope.InteractionToken.IsSome(out string? interactionToken))
            {
                _interactionCache.SetRetry(interactionToken, bucket, retry);
            }
            else if (scope.Webhook.IsSome(out Snowflake webhook))
            {
                _webhookCache.SetRetry(webhook, bucket, retry);
            }
            else if (scope.Guild.IsSome(out Snowflake guild))
            {
                _guildCache.SetRetry(guild, bucket, retry);
            }
            else
            {
                _globalCache.SetRetry(Unit.Default, bucket, retry);
            }
        }
    }

    private void UpdateBuckets(DiscordHttpRequest request, RateLimitHeaders headers)
    {
        if (!headers.Bucket.IsSome(out string? newBucket))
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

    private void LogRequestResponse(DiscordHttpRequest request,                 HttpStatusCode status, RateLimitHeaders headers,
                                    RateLimitContent?  rateLimitContent = null, ErrorContent?  errorContent = null)
    {
        HttpMethod method = request.Method;
        string path = request.GetPath();
        var statusNumber = (int)status;
        string? statusName = Enum.GetName(status);

        if (rateLimitContent is not null && rateLimitContent.RetryAfter.IsSome(out float retryAfter))
        {
            string bucketName;
            if (headers.Global.IsSome(out bool global) && global)
            {
                bucketName = "global";
            }
            else if (headers.Bucket.IsSome(out string? bucket))
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
        else if (headers.Remaining.IsSome(out int remaining) &&
            headers.Limit.IsSome(out int limit) &&
            headers.ResetAfter.IsSome(out double resetAfter))
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

    private record struct Endpoint(HttpMethod Method, string Url);

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
