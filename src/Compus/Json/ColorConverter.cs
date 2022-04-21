using System;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Compus.Json;

internal class ColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int c;
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
            {
                c = reader.GetInt32();
                break;
            }
            case JsonTokenType.String when options.NumberHandling.HasFlag(JsonNumberHandling.AllowReadingFromString):
            {
                c = int.Parse(reader.GetString()!);
                break;
            }
            default:
            {
                throw new JsonException();
            }
        }

        return Color.FromArgb((int)(c | 0xFF000000));
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        int c = value.ToArgb();
        writer.WriteNumberValue(c);
    }
}
