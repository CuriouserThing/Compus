namespace Compus.Gateway
{
    internal record ResumeData
    {
        public string Token { get; init; }

        public string SessionId { get; init; }

        public int Seq { get; init; }
    }
}
