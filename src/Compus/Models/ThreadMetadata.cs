using System;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#thread-metadata-object
    /// </summary>
    public record ThreadMetadata
    {
        public bool Archived { get; init; }

        public int AutoArchiveDuration { get; init; }

        public DateTime ArchiveTimestamp { get; init; }

        public Option<bool> Locked { get; init; }
    }
}
