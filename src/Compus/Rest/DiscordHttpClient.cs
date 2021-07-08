using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Compus.Caching;
using Compus.Equality;
using Compus.Json;
using Microsoft.Extensions.Logging;

namespace Compus.Rest
{
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

        private readonly object _cacheLock = new();
        private readonly Cache<(Snowflake, string), RateLimit> _channelBuckets = new(SnowflakeBucketComparer, new LifespanEvictionPolicy(ChannelBucketLifespan));
        private readonly HttpClient _client = new();
        private readonly Dictionary<(HttpMethod, string), string[]> _endpointBucketHashes = new();
        private readonly Dictionary<string, RateLimit> _globalBuckets = new();
        private readonly Cache<(Snowflake, string), RateLimit> _guildBuckets = new(SnowflakeBucketComparer, new LifespanEvictionPolicy(GuildBucketLifespan));
        private readonly Cache<(string, string), RateLimit> _interactionBuckets = new(TokenBucketComparer, new LifespanEvictionPolicy(InteractionBucketLifespan));
        private readonly ILogger _logger;
        private readonly string _token;
        private readonly Cache<(Snowflake, string), RateLimit> _webhookBuckets = new(SnowflakeBucketComparer, new LifespanEvictionPolicy(WebhookBucketLifespan));

        private DateTime _globalRetryTime;

        public DiscordHttpClient(string token, ILogger logger)
        {
            _token  = token;
            _logger = logger;
        }

        private static IEqualityComparer<(Snowflake, string)> SnowflakeBucketComparer { get; } = new Identity<(Snowflake, string)>()
            .With(resource => resource.Item1)
            .With(resource => resource.Item2)
            .ToComparer();

        private static IEqualityComparer<(string, string)> TokenBucketComparer { get; } = new Identity<(string, string)>()
            .With(resource => resource.Item1)
            .With(resource => resource.Item2)
            .ToComparer();

        public async Task<HttpResponseMessage> Send(DiscordHttpRequest request, CancellationToken cancellationToken)
        {
            while (true)
            {
                DateTime retryTime = GetCachedRetryTime(request);
                TimeSpan waitTime = retryTime - DateTime.UtcNow;
                if (waitTime > TimeSpan.Zero)
                {
                    await Task.Delay(waitTime, cancellationToken);
                    continue;
                }

                string path = request.GetPath();
                var req = new HttpRequestMessage
                {
                    Method     = request.Method,
                    RequestUri = new Uri($"{Scheme}://{Host}{BasePath}{ApiVersion}{path}"),
                    Content    = request.Content,
                };
                req.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationScheme, _token);
                HttpResponseMessage response = await _client.SendAsync(req, cancellationToken);
                HttpStatusCode status = response.StatusCode;

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
                        _logger.LogInformation("Couldn't deserialize rate limit response content. Ignoring content.", ex);
                    }
                }

                RateLimitHeaders headers = GetRateLimitHeaders(response, rateLimitContent);
                CacheRetryTime(request, status, headers);
                LogRequestResponse(request, status, headers);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                else if (status == HttpStatusCode.TooManyRequests)
                {
                    response.Dispose();
                }
                else
                {
                    ErrorContent ec;
                    try
                    {
                        await using Stream responseContent = await response.Content.ReadAsStreamAsync(cancellationToken);
                        ec = await JsonSerializer.DeserializeAsync<ErrorContent>(responseContent, JsonOptions.SerializerOptions, cancellationToken)
                             ?? throw new JsonException();
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning($"Discord returned the HTTP code {status}, but we couldn't deserialize error content from the response.", ex);
                        throw new DiscordApiException("Unknown Discord API error encountered.", status, ex);
                    }

                    response.Dispose();
                    throw new DiscordApiException(ec.Message, status)
                    {
                        Code   = ec.Code,
                        Errors = ec.Errors,
                    };
                }
            }
        }

        private RateLimitHeaders GetRateLimitHeaders(HttpResponseMessage httpResponse, RateLimitContent? rateLimitContent = null)
        {
            Option<bool> global = default;
            Option<int> limit = default;
            Option<int> remaining = default;
            Option<DateTimeOffset> reset = default;
            Option<TimeSpan> resetAfter = default;
            Option<string> bucket = default;

            if (rateLimitContent is not null && rateLimitContent.Global.IsSome(out bool bGlobal))
            {
                global = bGlobal;
            }
            else if (TryGetRateLimitHeader(httpResponse, "Global", out string? sGlobal))
            {
                if (bool.TryParse(sGlobal, out bool b))
                {
                    global = b;
                }
                else
                {
                    _logger.LogWarning("Couldn't parse Global rate limit header as boolean. Ignoring it.");
                }
            }

            if (TryGetRateLimitHeader(httpResponse, "Limit", out string? sLimit))
            {
                if (int.TryParse(sLimit, out int n))
                {
                    limit = n;
                }
                else
                {
                    _logger.LogWarning("Couldn't parse Limit rate limit header as integer. Ignoring it.");
                }
            }

            if (TryGetRateLimitHeader(httpResponse, "Remaining", out string? sRemaining))
            {
                if (int.TryParse(sRemaining, out int n))
                {
                    remaining = n;
                }
                else
                {
                    _logger.LogWarning("Couldn't parse Remaining rate limit header as integer. Ignoring it.");
                }
            }

            if (TryGetRateLimitHeader(httpResponse, "Reset", out string? sReset))
            {
                if (double.TryParse(sReset, out double n))
                {
                    reset = DateTimeOffset.UnixEpoch + TimeSpan.FromSeconds(n);
                }
                else
                {
                    _logger.LogWarning("Couldn't parse Reset rate limit header as floating-point. Ignoring it.");
                }
            }

            if (rateLimitContent is not null && rateLimitContent.RetryAfter.IsSome(out float fRetryAfter))
            {
                resetAfter = TimeSpan.FromSeconds(fRetryAfter);
            }
            else if (TryGetRateLimitHeader(httpResponse, "Reset-After", out string? sResetAfter))
            {
                if (double.TryParse(sResetAfter, out double n))
                {
                    resetAfter = TimeSpan.FromSeconds(n);
                }
                else
                {
                    _logger.LogWarning("Couldn't parse Reset-After rate limit header as floating-point. Ignoring it.");
                }
            }

            if (TryGetRateLimitHeader(httpResponse, "Bucket", out string? sBucket))
            {
                bucket = sBucket;
            }

            return new RateLimitHeaders
            {
                Global     = global,
                Limit      = limit,
                Remaining  = remaining,
                Reset      = reset,
                ResetAfter = resetAfter,
                Bucket     = bucket,
            };
        }

        private bool TryGetRateLimitHeader(HttpResponseMessage response, string key, [NotNullWhen(true)] out string? value)
        {
            key = $"X-RateLimit-{key}";
            if (!response.Headers.TryGetValues(key, out IEnumerable<string>? values))
            {
                _logger.LogTrace($"No values found for header {key}.");
                value = null;
                return false;
            }

            string[] v = values.ToArray();
            if (v.Length != 1)
            {
                _logger.LogWarning($"Multiple values found for header {key}. Ignoring all of them.");
                value = null;
                return false;
            }

            value = v[0];
            return true;
        }

        private DateTime GetCachedRetryTime(DiscordHttpRequest request)
        {
            DateTime retryTime;
            lock (_cacheLock)
            {
                retryTime = _globalRetryTime;
                if (!_endpointBucketHashes.TryGetValue((request.Method, request.Endpoint), out var buckets))
                {
                    return retryTime;
                }

                void CheckLimits<T>(T resource, IDictionary<(T, string), Cached<RateLimit>> resourceBuckets)
                {
                    foreach (string bucket in buckets)
                    {
                        if (resourceBuckets.TryGetValue((resource, bucket), out var cached))
                        {
                            DateTime t = cached.Item.RetryTime;
                            retryTime = t > retryTime ? t : retryTime;
                        }
                    }
                }

                ResourceScope scope = request.Scope;
                if (scope.Channel.IsSome(out Snowflake channel))
                {
                    CheckLimits(channel, _channelBuckets);
                }
                else if (scope.InteractionToken.IsSome(out string? interactionToken))
                {
                    CheckLimits(interactionToken, _interactionBuckets);
                }
                else if (scope.Webhook.IsSome(out Snowflake webhook))
                {
                    CheckLimits(webhook, _webhookBuckets);
                }
                else if (scope.Guild.IsSome(out Snowflake guild))
                {
                    CheckLimits(guild, _guildBuckets);
                }
                else
                {
                    foreach (string bucket in buckets)
                    {
                        if (_globalBuckets.TryGetValue(bucket, out RateLimit limit))
                        {
                            DateTime t = limit.RetryTime;
                            retryTime = t > retryTime ? t : retryTime;
                        }
                    }
                }
            }

            return retryTime;
        }

        private void CacheRetryTime(DiscordHttpRequest request, HttpStatusCode status, RateLimitHeaders headers)
        {
            DateTime now = DateTime.UtcNow;
            DateTime resetTime;
            if (headers.ResetAfter.IsSome(out TimeSpan resetAfter))
            {
                resetTime = now + resetAfter;
            }
            else if (headers.Reset.IsSome(out DateTimeOffset reset))
            {
                resetTime = reset.UtcDateTime;
            }
            else
            {
                resetTime = now;
            }

            if (headers.Global.IsSome(out bool global) && global)
            {
                lock (_cacheLock)
                {
                    _globalRetryTime = resetTime;
                }
            }
            else if (headers.Bucket.IsSome(out string? bucket))
            {
                if (!headers.Remaining.IsSome(out int remaining))
                {
                    // If this header isn't present for some reason, we can fabricate a somewhat-logical one.
                    remaining = status == HttpStatusCode.TooManyRequests ? 0 : 1;
                }

                ResourceScope scope = request.Scope;
                RateLimit rateLimit = new(status == HttpStatusCode.TooManyRequests, remaining, resetTime);
                var bucketIsNew = false;
                lock (_cacheLock)
                {
                    if (scope.Channel.IsSome(out Snowflake channel))
                    {
                        _channelBuckets.Add((channel, bucket), new Cached<RateLimit>(rateLimit, now));
                    }
                    else if (scope.InteractionToken.IsSome(out string? interactionToken))
                    {
                        _interactionBuckets.Add((interactionToken, bucket), new Cached<RateLimit>(rateLimit, now));
                    }
                    else if (scope.Webhook.IsSome(out Snowflake webhook))
                    {
                        _webhookBuckets.Add((webhook, bucket), new Cached<RateLimit>(rateLimit, now));
                    }
                    else if (scope.Guild.IsSome(out Snowflake guild))
                    {
                        _guildBuckets.Add((guild, bucket), new Cached<RateLimit>(rateLimit, now));
                    }
                    else
                    {
                        _globalBuckets[bucket] = rateLimit;
                    }

                    if (!_endpointBucketHashes.TryGetValue((request.Method, request.Endpoint), out var buckets))
                    {
                        bucketIsNew = true;
                        _endpointBucketHashes.Add((request.Method, request.Endpoint), new[] { bucket });
                    }
                    else if (!buckets.Contains(bucket))
                    {
                        bucketIsNew                                               = true;
                        _endpointBucketHashes[(request.Method, request.Endpoint)] = buckets.Append(bucket).ToArray();
                    }
                }

                if (bucketIsNew)
                {
                    _logger.LogInformation($"Registered endpoint {request.Method} {request.Endpoint} to bucket {bucket}.");
                }
            }
        }

        private void LogRequestResponse(DiscordHttpRequest request, HttpStatusCode status, RateLimitHeaders headers)
        {
            StringBuilder log = new();
            HttpMethod method = request.Method;
            string path = request.GetPath();
            log.Append($"{method} {path} -> {(int)status} {Enum.GetName(status)}.");

            if (status == HttpStatusCode.TooManyRequests)
            {
                if (headers.Global.IsSome(out bool global) && global)
                {
                    log.Append(" Globally rate limited");
                }
                else if (headers.Bucket.IsSome(out string? newBucket))
                {
                    log.Append($" Rate limited at bucket {newBucket}");
                }
                else
                {
                    log.Append(" Rate limited at unknown bucket");
                }

                if (headers.ResetAfter.IsSome(out TimeSpan resetAfter))
                {
                    log.Append($"; retry after {resetAfter}");
                }
                else if (headers.Reset.IsSome(out DateTimeOffset reset))
                {
                    log.Append($"; retry at {reset}");
                }

                log.Append('.');
            }
            else
            {
                if (headers.Remaining.IsSome(out int remaining))
                {
                    log.Append($" {remaining}");
                    if (headers.Limit.IsSome(out int limit)) { log.Append($"/{limit}"); }

                    log.Append(" remaining");
                    if (headers.ResetAfter.IsSome(out TimeSpan resetAfter))
                    {
                        log.Append($"; resets after {resetAfter}");
                    }
                    else if (headers.Reset.IsSome(out DateTimeOffset reset))
                    {
                        log.Append($"; resets at {reset}");
                    }

                    log.Append('.');
                }
            }

            _logger.LogDebug(log.ToString());
        }

        private readonly struct RateLimit
        {
            private readonly bool _wasLimited;
            private readonly int _remaining;
            private readonly DateTime _reset;

            public RateLimit(bool wasLimited, int remaining, DateTime reset)
            {
                _wasLimited = wasLimited;
                _remaining  = remaining;
                _reset      = reset;
            }

            public DateTime RetryTime
            {
                get
                {
                    if (_remaining < 1 || _wasLimited)
                    {
                        return _reset;
                    }
                    else
                    {
                        return default(DateTime);
                    }
                }
            }
        }

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
}
