using System;

namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/resources/guild-scheduled-event#guild-scheduled-event-object
/// </summary>
public record GuildScheduledEvent
{
    public Snowflake Id { get; init; }

    public Snowflake GuildId { get; init; }

    public Snowflake? ChannelId { get; init; }

    public NullableOption<Snowflake> CreatorId { get; init; }

    public string Name { get; init; }

    public NullableOption<string> Description { get; init; }

    public DateTimeOffset ScheduledStartTime { get; init; }

    public DateTimeOffset? ScheduledEndTime { get; init; }

    public PrivacyLevel PrivacyLevel { get; init; }

    public GuildScheduledEventStatus Status { get; init; }

    public GuildScheduledEventEntityType EntityType { get; init; }

    public Snowflake? EntityId { get; init; }

    public GuildScheduledEventEntityMetadata? EntityMetadata { get; init; }

    public Option<User> Creator { get; init; }

    public Option<int> UserCount { get; init; }

    public NullableOption<string> Image { get; init; }
}
