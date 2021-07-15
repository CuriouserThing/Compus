namespace Compus.Gateway.Events
{
    public record MessageDeleteEventArgs
    {
        public Snowflake Id { get; init; }

        public Snowflake ChannelId { get; init; }

        public Option<Snowflake> GuildId { get; init; }
    }
}
