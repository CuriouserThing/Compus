namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#embed-object-embed-thumbnail-structure
    /// </summary>
    public record EmbedThumbnail
    {
        public Option<string> Url { get; init; }

        public Option<string> ProxyUrl { get; init; }

        public Option<int> Height { get; init; }

        public Option<int> Width { get; init; }
    }
}
