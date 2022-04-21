namespace Compus.Models;

public record WelcomeScreenChannel
{
    public Snowflake ChannelId { get; init; }

    public string Description { get; init; }

    public Snowflake? EmojiId { get; init; }

    public string? EmojiName { get; init; }
}
