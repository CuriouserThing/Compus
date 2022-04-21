namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/resources/guild-scheduled-event#guild-scheduled-event-object-guild-scheduled-event-status
/// </summary>
public enum GuildScheduledEventStatus
{
    Scheduled = 1,
    Active = 2,
    Completed = 3,
    Canceled = 4,
}
