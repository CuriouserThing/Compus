namespace Compus.Gateway
{
    public record Shard
    {
        public Shard()
        {
        }

        public Shard(int shardId, int numShards)
        {
            ShardId   = shardId;
            NumShards = numShards;
        }

        public int ShardId { get; init; }

        public int NumShards { get; init; } = 1;

        public override string ToString()
        {
            return $"[{ShardId}, {NumShards}]";
        }
    }
}
