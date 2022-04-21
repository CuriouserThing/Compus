namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/resources/guild-scheduled-event#guild-scheduled-event-object-guild-scheduled-event-entity-metadata
/// </summary>
public record GuildScheduledEventEntityMetadata
{
    public Option<string> Location { get; init; }
}
