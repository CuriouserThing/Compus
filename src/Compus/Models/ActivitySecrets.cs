namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/topics/gateway#activity-object-activity-secrets
/// </summary>
public record ActivitySecrets
{
    public Option<string> Join { get; init; }

    public Option<string> Spectate { get; init; }

    public Option<string> Match { get; init; }
}
