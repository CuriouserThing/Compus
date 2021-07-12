namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#message-object-message-sticker-item-structure
    /// </summary>
    public record MessageStickerItem
    {
        public Snowflake Id { get; init; }

        public string Name { get; init; }

        public MessageStickerFormatType FormatType { get; init; }
    }
}
