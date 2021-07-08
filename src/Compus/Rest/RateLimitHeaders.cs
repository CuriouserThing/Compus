using System;

namespace Compus.Rest
{
    /// <summary>
    ///     https://discord.com/developers/docs/topics/rate-limits#header-format
    /// </summary>
    internal class RateLimitHeaders
    {
        /// <summary>
        ///     Returned only on a HTTP 429 response if the rate limit headers returned are of the global rate limit (not
        ///     per-route)
        /// </summary>
        public Option<bool> Global { get; init; }

        /// <summary>
        ///     The number of requests that can be made.
        /// </summary>
        public Option<int> Limit { get; init; }

        /// <summary>
        ///     The number of remaining requests that can be made.
        /// </summary>
        public Option<int> Remaining { get; init; }

        /// <summary>
        ///     Time at which the rate limit resets.
        /// </summary>
        public Option<DateTimeOffset> Reset { get; init; }

        /// <summary>
        ///     Total time of when the current rate limit bucket will reset.
        /// </summary>
        public Option<TimeSpan> ResetAfter { get; init; }

        /// <summary>
        ///     A unique string denoting the rate limit being encountered (non-inclusive of major parameters in the route path).
        /// </summary>
        public Option<string> Bucket { get; init; }
    }
}
