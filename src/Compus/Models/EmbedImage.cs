namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#embed-object-embed-image-structure
    /// </summary>
    public record EmbedImage
    {
        public Option<string> Url { get; init; }

        public Option<string> ProxyUrl { get; init; }

        public Option<int> Height { get; init; }

        public Option<int> Width { get; init; }
    }
}
