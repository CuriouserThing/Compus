namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#embed-object-embed-provider-structure
    /// </summary>
    public record EmbedProvider
    {
        public Option<string> Name { get; init; }

        public Option<string> Url { get; init; }
    }
}
