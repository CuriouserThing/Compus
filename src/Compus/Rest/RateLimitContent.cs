namespace Compus.Rest
{
    internal record RateLimitContent
    {
        public Option<string> Message { get; init; }

        public Option<float> RetryAfter { get; init; }

        public Option<bool> Global { get; init; }
    }
}
