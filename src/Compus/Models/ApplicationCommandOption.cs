using System.Collections.Generic;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/slash-commands#application-command-object-application-command-option-structure
    /// </summary>
    public record ApplicationCommandOption
    {
        public ApplicationCommandOptionType Type { get; init; }

        public string Name { get; init; }

        public string Description { get; init; }

        public Option<bool> Required { get; init; }

        public Option<IReadOnlyList<ApplicationCommandOptionChoice>> Choices { get; init; }

        public Option<IReadOnlyList<ApplicationCommandOption>> Options { get; init; }
    }
}
