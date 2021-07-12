namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#message-object-message-activity-structure
    /// </summary>
    public record MessageActivity
    {
        public MessageActivityType Type { get; init; }

        public Option<string> PartyId { get; init; }
    }
}
