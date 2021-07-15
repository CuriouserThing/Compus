using System.Collections.Generic;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/slash-commands#interaction-object-application-command-interaction-data-resolved-structure
    /// </summary>
    public record ApplicationCommandInteractionDataResolved
    {
        public Option<IReadOnlyDictionary<Snowflake, User>> Users { get; init; }

        /// <summary>
        /// Missing <see cref="GuildMember.User"/>, <see cref="GuildMember.Deaf"/>, and <see cref="GuildMember.Mute"/> fields.
        /// </summary>
        public Option<IReadOnlyDictionary<Snowflake, GuildMember>> Members { get; init; }

        public Option<IReadOnlyDictionary<Snowflake, Role>> Roles { get; init; }

        /// <summary>
        /// Only has <see cref="Channel.Id"/>, <see cref="Channel.Name"/>, <see cref="Channel.Type"/>, and <see cref="Channel.PermissionOverwrites"/> fields.
        /// </summary>
        public Option<IReadOnlyDictionary<Snowflake, Channel>> Channels { get; init; }
    }
}
