using System.Collections.Generic;

namespace Compus.Gateway.Events
{
    public record MessageDeleteBulkEventArgs
    {
        public IReadOnlyList<Snowflake> Ids { get; init; }

        public Snowflake ChannelId { get; init; }

        public Option<Snowflake> GuildId { get; init; }
    }
}
