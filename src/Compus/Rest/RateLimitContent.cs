namespace Compus.Rest;

/// <summary>
///     https://discord.com/developers/docs/topics/rate-limits#exceeding-a-rate-limit-rate-limit-response-structure
/// </summary>
internal record RateLimitContent
{
    /// <summary>
    ///     A message saying you are being rate limited.
    /// </summary>
    public Option<string> Message { get; init; }

    /// <summary>
    ///     The number of seconds to wait before submitting another request.
    /// </summary>
    public Option<float> RetryAfter { get; init; }

    /// <summary>
    ///     A value indicating if you are being globally rate limited or not
    /// </summary>
    public Option<bool> Global { get; init; }
}
