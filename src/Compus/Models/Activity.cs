using System.Collections.Generic;

namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/topics/gateway#activity-object
/// </summary>
public record Activity
{
    public string Name { get; init; }

    public ActivityType Type { get; init; }

    public NullableOption<string> Url { get; init; }

    public int CreatedAt { get; init; }

    public Option<ActivityTimestamps> Timestamps { get; init; }

    public Option<Snowflake> ApplicationId { get; init; }

    public NullableOption<string> Details { get; init; }

    public NullableOption<string> State { get; init; }

    public NullableOption<ActivityEmoji> Emoji { get; init; }

    public Option<ActivityParty> Party { get; init; }

    public Option<ActivityAssets> Assets { get; init; }

    public Option<ActivitySecrets> Secrets { get; init; }

    public Option<bool> Instance { get; init; }

    public Option<IReadOnlyList<ActivityButton>> Buttons { get; init; }
}
