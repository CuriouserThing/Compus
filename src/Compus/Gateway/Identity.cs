namespace Compus.Gateway
{
    public class Identity
    {
        public Identity(string token, GatewayIntents intents = GatewayIntents.None)
        {
            Token   = token;
            Intents = intents;
        }

        public string Token { get; }

        public GatewayIntents Intents { get; }

        public Option<Shard> Shard { get; init; }
    }
}
