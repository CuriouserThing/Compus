namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#embed-object-embed-author-structure
    /// </summary>
    public record EmbedAuthor
    {
        public Option<string> Name { get; init; }

        public Option<string> Url { get; init; }

        public Option<string> IconUrl { get; init; }

        public Option<string> ProxyIconUrl { get; init; }
    }
}
