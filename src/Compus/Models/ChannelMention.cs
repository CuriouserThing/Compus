namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#channel-mention-object
    /// </summary>
    public record ChannelMention
    {
        public Snowflake Id { get; init; }

        public Snowflake GuildId { get; init; }

        public ChannelType Type { get; init; }

        public string Name { get; init; }
    }
}
