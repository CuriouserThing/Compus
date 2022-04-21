namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/resources/stage-instance#stage-instance-object
/// </summary>
public record StageInstance
{
    public Snowflake Id { get; init; }

    public Snowflake GuildId { get; init; }

    public Snowflake ChannelId { get; init; }

    public string Topic { get; init; }

    public PrivacyLevel PrivacyLevel { get; init; }

    public bool DiscoverableDisabled { get; init; }

    public Snowflake? GuildScheduledEventId { get; init; }
}
