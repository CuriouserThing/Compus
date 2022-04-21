using System.Collections.Generic;

namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/topics/gateway#activity-object-activity-party
/// </summary>
public record ActivityParty
{
    public Option<string> Id { get; init; }

    public Option<IReadOnlyList<int>> Size { get; init; }
}
