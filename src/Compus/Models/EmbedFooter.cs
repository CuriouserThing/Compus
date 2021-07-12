namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#embed-object-embed-footer-structure
    /// </summary>
    public record EmbedFooter
    {
        public string Text { get; init; }

        public Option<string> IconUrl { get; init; }

        public Option<string> ProxyIconUrl { get; init; }
    }
}
