namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/resources/sticker#sticker-object
/// </summary>
public record Sticker
{
    public Snowflake Id { get; init; }

    public Option<Snowflake> PackId { get; init; }

    public string Name { get; init; }

    public string? Description { get; init; }

    public string Tags { get; init; }

    public Option<string> Asset { get; init; }

    public StickerType Type { get; init; }

    public StickerFormatType FormatType { get; init; }

    public Option<bool> Available { get; init; }

    public Option<Snowflake> GuildId { get; init; }

    public Option<User> User { get; init; }

    public Option<int> SortValue { get; init; }
}
