namespace Compus.Gateway
{
    public class Identity
    {
        public Identity(Token token, GatewayIntents intents)
        {
            Token   = token;
            Intents = intents;
        }

        public Identity(Token token, GatewayIntents intents, Option<Shard> shard)
        {
            Token   = token;
            Intents = intents;
            Shard = shard;
        }

        public Token Token { get; }

        public GatewayIntents Intents { get; }

        public Option<Shard> Shard { get; }
    }
}
