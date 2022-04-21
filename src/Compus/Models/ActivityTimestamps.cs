namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/topics/gateway#activity-object-activity-timestamps
/// </summary>
public record ActivityTimestamps
{
    public Option<int> Start { get; init; }

    public Option<int> End { get; init; }
}
