using System;
using System.Collections.Generic;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/guild#guild-member-object
    /// </summary>
    public record GuildMember
    {
        public Option<User> User { get; init; }

        public NullableOption<string> Nick { get; init; }

        public IReadOnlyList<Snowflake> Roles { get; init; }

        public DateTimeOffset JoinedAt { get; init; }

        public NullableOption<DateTimeOffset> PremiumSince { get; init; }

        public bool Deaf { get; init; }

        public bool Mute { get; init; }

        public Option<bool> Pending { get; init; }

        public Option<PermissionSet> Permissions { get; init; }
    }
}
