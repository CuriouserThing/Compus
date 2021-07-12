using System.Collections.Generic;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#allowed-mentions-object
    /// </summary>
    public record AllowedMentions
    {
        public Option<IReadOnlyList<string>> Parse { get; init; }

        public Option<IReadOnlyList<Snowflake>> Roles { get; init; }

        public Option<IReadOnlyList<Snowflake>> Users { get; init; }

        public Option<bool> RepliedUser { get; init; }
    }
}
