namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/topics/gateway#activity-object-activity-emoji
/// </summary>
public record ActivityEmoji
{
    public string Name { get; init; }

    public Option<Snowflake> Id { get; init; }

    public Option<bool> Animated { get; init; }
}
