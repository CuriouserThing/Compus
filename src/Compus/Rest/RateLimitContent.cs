namespace Compus.Rest
{
    internal class RateLimitContent
    {
        public Option<string> Message { get; init; }

        public Option<float> RetryAfter { get; init; }

        public Option<bool> Global { get; init; }
    }
}
