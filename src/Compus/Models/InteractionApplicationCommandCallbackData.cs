using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/slash-commands#interaction-response-object-interaction-callback-type
    /// </summary>
    public record InteractionApplicationCommandCallbackData
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<bool> Tts { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<string> Content { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<IReadOnlyList<Embed>> Embeds { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<AllowedMentions> AllowedMentions { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<InteractionApplicationCommandCallbackDataFlags> Flags { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<IReadOnlyList<Component>> Components { get; init; }
    }
}
