using System.Collections.Generic;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/slash-commands#interaction-object-application-command-interaction-data-option-structure
    /// </summary>
    public record ApplicationCommandInteractionDataOption
    {
        public string Name { get; init; }

        public ApplicationCommandOptionType Type { get; init; }

        public Option<string> Value { get; init; }

        public Option<IReadOnlyList<ApplicationCommandInteractionDataOption>> Options { get; init; }
    }
}
