using System;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/slash-commands#interaction-response-object-interaction-application-command-callback-data-flags
    /// </summary>
    [Flags]
    public enum InteractionApplicationCommandCallbackDataFlags
    {
        Ephemeral = 1 << 6,
    }
}
