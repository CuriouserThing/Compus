namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#embed-object-embed-video-structure
    /// </summary>
    public record EmbedVideo
    {
        public Option<string> Url { get; init; }

        public Option<string> ProxyUrl { get; init; }

        public Option<int> Height { get; init; }

        public Option<int> Width { get; init; }
    }
}
