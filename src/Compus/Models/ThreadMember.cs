using System;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#thread-member-object
    /// </summary>
    public record ThreadMember
    {
        public Snowflake Id { get; init; }

        public Snowflake UserId { get; init; }

        public DateTime JoinTimestamp { get; init; }

        public int Flags { get; init; }
    }
}
