using System;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#message-object-message-flags
    /// </summary>
    [Flags]
    public enum MessageFlags
    {
        Crossposted = 1          << 0,
        IsCrosspost = 1          << 1,
        SuppressEmbeds = 1       << 2,
        SourceMessageDeleted = 1 << 3,
        Urgent = 1               << 4,
        HasThread = 1            << 5,
        Ephemeral = 1            << 6,
        Loading = 1              << 7,
    }
}
