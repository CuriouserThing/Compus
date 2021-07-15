using System.Text.Json.Serialization;

namespace Compus.Gateway
{
    internal record IdentifyData
    {
        public string Token { get; init; }

        public IdentifyConnectionProperties Properties { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<bool> Compress { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<int> LargeThreshold { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<Shard> Shard { get; init; }

        public GatewayIntents Intents { get; init; }
    }
}
