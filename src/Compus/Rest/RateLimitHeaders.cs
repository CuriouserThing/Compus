using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Compus.Rest;

/// <summary>
///     https://discord.com/developers/docs/topics/rate-limits#header-format
/// </summary>
internal class RateLimitHeaders
{
    /// <summary>
    ///     The number of requests that can be made
    /// </summary>
    public Option<int> Limit { get; init; }

    /// <summary>
    ///     The number of remaining requests that can be made
    /// </summary>
    public Option<int> Remaining { get; init; }

    /// <summary>
    ///     Epoch time (seconds since 00:00:00 UTC on January 1, 1970) at which the rate limit resets
    /// </summary>
    public Option<double> Reset { get; init; }

    /// <summary>
    ///     Total time (in seconds) of when the current rate limit bucket will reset. Can have decimals to match previous
    ///     millisecond ratelimit precision
    /// </summary>
    public Option<double> ResetAfter { get; init; }

    /// <summary>
    ///     A unique string denoting the rate limit being encountered (non-inclusive of top-level resources in the path)
    /// </summary>
    public Option<string> Bucket { get; init; }

    /// <summary>
    ///     Returned only on HTTP 429 responses if the rate limit encountered is the global rate limit (not per-route)
    /// </summary>
    public Option<bool> Global { get; init; }

    /// <summary>
    ///     Returned only on HTTP 429 responses. Value can be <c>user</c> (per bot or user limit), <c>global</c> (per bot or
    ///     user global limit), or <c>shared</c> (per resource limit)
    /// </summary>
    public Option<string> Scope { get; init; }

    public static RateLimitHeaders ReadFromResponse(HttpResponseMessage httpResponse, ILogger logger)
    {
        Option<int> limit = default;
        if (TryGetRateLimitHeader(httpResponse, logger, "Limit", out string? sLimit))
        {
            if (int.TryParse(sLimit, out int n))
            {
                limit = n;
            }
            else
            {
                logger.LogWarning("Couldn't parse Limit rate limit header as integer. Ignoring it.");
            }
        }

        Option<int> remaining = default;
        if (TryGetRateLimitHeader(httpResponse, logger, "Remaining", out string? sRemaining))
        {
            if (int.TryParse(sRemaining, out int n))
            {
                remaining = n;
            }
            else
            {
                logger.LogWarning("Couldn't parse Remaining rate limit header as integer. Ignoring it.");
            }
        }

        Option<double> reset = default;
        if (TryGetRateLimitHeader(httpResponse, logger, "Reset", out string? sReset))
        {
            if (double.TryParse(sReset, out double n))
            {
                reset = n;
            }
            else
            {
                logger.LogWarning("Couldn't parse Reset rate limit header as floating-point. Ignoring it.");
            }
        }

        Option<double> resetAfter = default;
        if (TryGetRateLimitHeader(httpResponse, logger, "Reset-After", out string? sResetAfter))
        {
            if (double.TryParse(sResetAfter, out double n))
            {
                resetAfter = n;
            }
            else
            {
                logger.LogWarning("Couldn't parse Reset-After rate limit header as floating-point. Ignoring it.");
            }
        }

        Option<string> bucket = default;
        if (TryGetRateLimitHeader(httpResponse, logger, "Bucket", out string? sBucket))
        {
            bucket = sBucket;
        }

        Option<bool> global = default;
        if (TryGetRateLimitHeader(httpResponse, logger, "Global", out string? sGlobal))
        {
            if (bool.TryParse(sGlobal, out bool b))
            {
                global = b;
            }
            else
            {
                logger.LogWarning("Couldn't parse Global rate limit header as boolean. Ignoring it.");
            }
        }

        Option<string> scope = default;
        if (TryGetRateLimitHeader(httpResponse, logger, "Scope", out string? sScope))
        {
            scope = sScope;
        }

        return new RateLimitHeaders
        {
            Limit = limit,
            Remaining = remaining,
            Reset = reset,
            ResetAfter = resetAfter,
            Bucket = bucket,
            Global = global,
            Scope = scope,
        };
    }

    private static bool TryGetRateLimitHeader(HttpResponseMessage response, ILogger logger, string key, [NotNullWhen(true)] out string? value)
    {
        key = $"X-RateLimit-{key}";
        if (!response.Headers.TryGetValues(key, out IEnumerable<string>? values))
        {
            logger.LogTrace("No values found for header {key}.", key);
            value = null;
            return false;
        }

        string[] v = values.ToArray();
        if (v.Length != 1)
        {
            logger.LogWarning("Multiple values found for header {key}. Ignoring all of them.", key);
            value = null;
            return false;
        }

        value = v[0];
        return true;
    }
}
