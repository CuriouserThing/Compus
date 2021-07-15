using Compus.Models;

namespace Compus.Gateway.Events
{
    public record MessageReactionAddEventArgs
    {
        public Snowflake UserId { get; init; }

        public Snowflake ChannelId { get; init; }

        public Snowflake MessageId { get; init; }

        public Option<Snowflake> GuildId { get; init; }

        public Option<GuildMember> Member { get; init; }

        public Emoji Emoji { get; init; }
    }
}
