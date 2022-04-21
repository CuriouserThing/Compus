using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Compus.Models;

namespace Compus.Json;

internal class PermissionSetConverter : JsonConverter<PermissionSet>
{
    public override PermissionSet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => new PermissionSet(reader.GetString()!),
            _                    => throw new JsonException(),
        };
    }

    public override void Write(Utf8JsonWriter writer, PermissionSet value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
