namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#reaction-object
    /// </summary>
    public record Reaction
    {
        public int Count { get; init; }

        public bool Me { get; init; }

        public Emoji Emoji { get; init; }
    }
}
