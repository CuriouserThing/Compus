using Compus.Models;

namespace Compus.Gateway.Events
{
    public record MessageReactionRemoveEmojiEventArgs
    {
        public Snowflake ChannelId { get; init; }

        public Option<Snowflake> GuildId { get; init; }

        public Snowflake MessageId { get; init; }

        public Emoji Emoji { get; init; }
    }
}
