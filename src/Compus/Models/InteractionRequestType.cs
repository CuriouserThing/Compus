namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/slash-commands#interaction-object-interaction-request-type
    /// </summary>
    public enum InteractionRequestType
    {
        Ping = 1,
        ApplicationCommand = 2,
        MessageComponent = 3,
    }
}
