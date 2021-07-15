namespace Compus.Gateway.Events
{
    public record MessageReactionRemoveAllEventArgs
    {
        public Snowflake ChannelId { get; init; }

        public Snowflake MessageId { get; init; }

        public Option<Snowflake> GuildId { get; init; }
    }
}
