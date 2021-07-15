using System.Text.Json.Serialization;

namespace Compus.Gateway
{
    internal record IdentifyConnectionProperties
    {
        [JsonPropertyName("$os")]
        public string Os { get; init; }

        [JsonPropertyName("$browser")]
        public string Browser { get; init; }

        [JsonPropertyName("$device")]
        public string Device { get; init; }
    }
}
