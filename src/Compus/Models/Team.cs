using System.Collections.Generic;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/topics/teams#data-models-team-object
    /// </summary>
    public record Team
    {
        public string? Icon { get; init; }

        public Snowflake Id { get; init; }

        public IReadOnlyList<TeamMember> Members { get; init; }

        public string Name { get; init; }

        public Snowflake OwnerUserId { get; init; }
    }
}
