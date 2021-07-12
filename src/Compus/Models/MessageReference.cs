namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#message-reference-object-message-reference-structure
    /// </summary>
    public record MessageReference
    {
        public Option<Snowflake> MessageId { get; init; }

        public Option<Snowflake> ChannelId { get; init; }

        public Option<Snowflake> GuildId { get; init; }

        public Option<bool> FailIfNotExists { get; init; }
    }
}
