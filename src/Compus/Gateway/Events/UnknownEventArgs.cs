using System.Text.Json;

namespace Compus.Gateway.Events
{
    public record UnknownEventArgs
    {
        public string Type { get; init; }

        public JsonElement Data { get; init; }
    }
}
