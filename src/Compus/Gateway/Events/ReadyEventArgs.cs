using System.Collections.Generic;
using Compus.Models;

namespace Compus.Gateway.Events
{
    public record ReadyEventArgs
    {
        public int V { get; init; }

        public User User { get; init; }

        public IReadOnlyList<UnavailableGuild> Guilds { get; init; }

        public string SessionId { get; init; }

        public Option<Shard> Shard { get; init; }

        public PartialApplication Application { get; init; }

        public class PartialApplication
        {
            public Snowflake Id { get; init; }

            public ApplicationFlags Flags { get; init; }
        }
    }
}
