namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/topics/gateway#get-gateway-bot-json-response
    /// </summary>
    public record BotGateway
    {
        public string Url { get; init; }

        public int Shards { get; init; }

        public SessionStartLimit SessionStartLimit { get; init; }
    }
}
