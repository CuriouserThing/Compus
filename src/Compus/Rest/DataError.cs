namespace Compus.Rest
{
    public record DataError
    {
        public string Code { get; init; }

        public string Message { get; init; }

        public Option<string> Path { get; init; }
    }
}
