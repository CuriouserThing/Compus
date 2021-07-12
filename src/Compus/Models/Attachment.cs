namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#attachment-object
    /// </summary>
    public record Attachment
    {
        public Snowflake Id { get; init; }

        public string Filename { get; init; }

        public string ContentType { get; init; }

        public int Size { get; init; }

        public string Url { get; init; }

        public string ProxyUrl { get; init; }

        public int? Height { get; init; }

        public int? Width { get; init; }
    }
}
