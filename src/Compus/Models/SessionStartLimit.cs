namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/topics/gateway#session-start-limit-object
    /// </summary>
    public record SessionStartLimit
    {
        public int Total { get; init; }

        public int Remaining { get; init; }

        public int ResetAfter { get; init; }

        public int MaxConcurrency { get; init; }
    }
}
