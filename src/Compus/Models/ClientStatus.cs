namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/topics/gateway#client-status-object
/// </summary>
public record ClientStatus
{
    public Option<string> Desktop { get; init; }

    public Option<string> Mobile { get; init; }

    public Option<string> Web { get; init; }
}
