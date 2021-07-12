namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/message-components#select-menu-object-select-option-structure
    /// </summary>
    public record SelectOption
    {
        public string Label { get; init; }

        public string Value { get; init; }

        public Option<string> Description { get; init; }

        public Option<Emoji> Emoji { get; init; }

        public Option<bool> Default { get; init; }
    }
}
