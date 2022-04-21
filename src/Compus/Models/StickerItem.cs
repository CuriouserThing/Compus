namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/resources/sticker#sticker-item-object
/// </summary>
public record StickerItem
{
    public Snowflake Id { get; init; }

    public string Name { get; init; }

    public StickerFormatType FormatType { get; init; }
}
