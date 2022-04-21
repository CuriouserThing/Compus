namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/topics/gateway#activity-object-activity-assets
/// </summary>
public record ActivityAssets
{
    public Option<string> LargeImage { get; init; }

    public Option<string> LargeText { get; init; }

    public Option<string> SmallImage { get; init; }

    public Option<string> SmallText { get; init; }
}
