using System.Text.Json.Serialization;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#embed-object-embed-field-structure
    /// </summary>
    public record EmbedField
    {
        public string Name { get; init; }

        public string Value { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<bool> Inline { get; init; }
    }
}
