namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/slash-commands#interaction-object
    /// </summary>
    public record Interaction
    {
        public Snowflake Id { get; init; }

        public Snowflake ApplicationId { get; init; }

        public InteractionRequestType Type { get; init; }

        public Option<ApplicationCommandInteractionData> Data { get; init; }

        public Option<Snowflake> GuildId { get; init; }

        public Option<Snowflake> ChannelId { get; init; }

        public Option<GuildMember> Member { get; init; }

        public Option<User> User { get; init; }

        public string Token { get; init; }

        public int Version { get; init; }

        public Option<Message> Message { get; init; }
    }
}
