using System.Collections.Generic;

namespace Compus.Models;

public record WelcomeScreen
{
    public string? Description { get; init; }

    public IReadOnlyList<WelcomeScreenChannel> WelcomeChannels { get; init; }
}
