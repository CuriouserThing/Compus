using System.Collections.Generic;
using System.Text.Json.Serialization;
using Compus.Models;

namespace Compus.Rest.Data
{
    public record MessageData
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<string> Content { get; init; } = null!;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<bool> Tts { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<IReadOnlyList<Embed>> Embeds { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<AllowedMentions> AllowedMentions { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<MessageReference> MessageReference { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<IReadOnlyList<Component>> Components { get; init; }
    }
}
