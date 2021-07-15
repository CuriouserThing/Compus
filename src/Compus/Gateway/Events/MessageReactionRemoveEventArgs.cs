using Compus.Models;

namespace Compus.Gateway.Events
{
    public record MessageReactionRemoveEventArgs
    {
        public Snowflake UserId { get; init; }

        public Snowflake ChannelId { get; init; }

        public Snowflake MessageId { get; init; }

        public Option<Snowflake> GuildId { get; init; }

        public Emoji Emoji { get; init; }
    }
}
