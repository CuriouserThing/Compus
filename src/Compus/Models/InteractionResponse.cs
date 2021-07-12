using System.Text.Json.Serialization;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/slash-commands#interaction-response-object
    /// </summary>
    public record InteractionResponse
    {
        public InteractionCallbackType Type { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<InteractionApplicationCommandCallbackData> Data { get; init; }
    }
}
