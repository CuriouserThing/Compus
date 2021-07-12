using System;
using System.Collections.Generic;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#channel-object
    /// </summary>
    public record Channel
    {
        public Snowflake Id { get; init; }

        public ChannelType Type { get; init; }

        public Option<Snowflake> GuildId { get; init; }

        public Option<int> Position { get; init; }

        public Option<IReadOnlyList<Overwrite>> PermissionOverwrites { get; init; }

        public Option<string> Name { get; init; }

        public NullableOption<string> Topic { get; init; }

        public Option<bool> Nsfw { get; init; }

        public NullableOption<Snowflake> LastMessageId { get; init; }

        public Option<int> Bitrate { get; init; }

        public Option<int> UserLimit { get; init; }

        public Option<int> RateLimitPerUser { get; init; }

        public Option<IReadOnlyList<User>> Recipients { get; init; }

        public NullableOption<string> Icon { get; init; }

        public Option<Snowflake> OwnerId { get; init; }

        public Option<Snowflake> ApplicationId { get; init; }

        public NullableOption<Snowflake> ParentId { get; init; }

        public NullableOption<DateTime> LastPinTimestamp { get; init; }

        public NullableOption<string> RtcRegion { get; init; }

        public Option<int> VideoQualityMode { get; init; }

        public Option<int> MessageCount { get; init; }

        public Option<int> MemberCount { get; init; }

        public Option<ThreadMetadata> ThreadMetadata { get; init; }

        public Option<ThreadMember> Member { get; init; }

        public Option<int> DefaultAutoArchiveDuration { get; init; }
    }
}
