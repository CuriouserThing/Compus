using System.Collections.Generic;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/topics/teams#data-models-team-members-object
    /// </summary>
    public record TeamMember
    {
        public MembershipState MembershipState { get; init; }

        public IReadOnlyList<string> Permissions { get; init; }

        public Snowflake TeamId { get; init; }

        public User User { get; init; }
    }
}
