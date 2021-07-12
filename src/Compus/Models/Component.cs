using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/message-components#component-object
    ///     https://discord.com/developers/docs/interactions/message-components#select-menu-object
    /// </summary>
    public record Component
    {
        public ComponentType Type { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<ButtonStyle> Style { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<string> Label { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<Emoji> Emoji { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<string> CustomId { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<string> Url { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<bool> Disabled { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<IReadOnlyList<Component>> Components { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<IReadOnlyList<SelectOption>> Options { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<string> Placeholder { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<int> MinValues { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<int> MaxValues { get; init; }
    }
}
