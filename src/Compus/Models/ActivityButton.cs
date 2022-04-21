namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/topics/gateway#activity-object-activity-buttons
/// </summary>
public record ActivityButton
{
    public string Label { get; init; }

    public string Url { get; init; }
}
