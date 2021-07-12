namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/slash-commands#message-interaction-object
    /// </summary>
    public record MessageInteraction
    {
        public Snowflake Id { get; init; }

        public InteractionRequestType RequestType { get; init; }

        public string Name { get; init; }

        public User User { get; init; }
    }
}
