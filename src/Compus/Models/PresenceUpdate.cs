using System.Collections.Generic;

namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/topics/gateway#presence-update
/// </summary>
public record PresenceUpdate
{
    public User User { get; init; }

    public Snowflake GuildId { get; init; }

    public string Status { get; init; }

    public IReadOnlyList<Activity> Activities { get; init; }

    public ClientStatus ClientStatus { get; init; }
}
