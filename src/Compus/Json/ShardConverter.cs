using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Compus.Gateway;

namespace Compus.Json
{
    internal class ShardConverter : JsonConverter<Shard>
    {
        public override Shard Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray &&
                reader.Read()                                &&
                reader.TryGetInt32(out int shardId)          &&
                reader.Read()                                &&
                reader.TryGetInt32(out int numShards)        &&
                reader.Read()                                &&
                reader.TokenType == JsonTokenType.EndArray)
            {
                return new Shard
                {
                    ShardId   = shardId,
                    NumShards = numShards,
                };
            }
            else
            {
                throw new JsonException();
            }
        }

        public override void Write(Utf8JsonWriter writer, Shard value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.ShardId);
            writer.WriteNumberValue(value.NumShards);
            writer.WriteEndArray();
        }
    }
}
