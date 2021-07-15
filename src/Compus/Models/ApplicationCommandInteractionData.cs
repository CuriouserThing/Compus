using System.Collections.Generic;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/slash-commands#interaction-object-application-command-interaction-data-structure
    /// </summary>
    public record ApplicationCommandInteractionData
    {
        public Snowflake Id { get; init; }

        public string Name { get; init; }

        public Option<ApplicationCommandInteractionDataResolved> Resolved { get; init; }

        public Option<IReadOnlyList<ApplicationCommandInteractionDataOption>> Options { get; init; }

        public string CustomId { get; init; }

        public ComponentType ComponentType { get; init; }
    }
}
