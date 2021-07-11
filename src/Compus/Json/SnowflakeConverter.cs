using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Compus.Json
{
    internal class SnowflakeConverter : JsonConverter<Snowflake>
    {
        public override Snowflake Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                {
                    return reader.GetUInt64();
                }
                case JsonTokenType.String when options.NumberHandling.HasFlag(JsonNumberHandling.AllowReadingFromString):
                {
                    return ulong.Parse(reader.GetString()!);
                }
                default:
                {
                    throw new JsonException();
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, Snowflake value, JsonSerializerOptions options)
        {
            ulong id = value;
            writer.WriteStringValue(id.ToString());
        }
    }
}
