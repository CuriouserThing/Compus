using System.Buffers;
using System.Text.Json;

namespace Compus.Json
{
    internal static class JsonExt
    {
        // Temporary workaround for:
        // https://github.com/dotnet/runtime/issues/40393
        // https://github.com/dotnet/runtime/issues/45188
        public static T? ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var jsonWriter = new Utf8JsonWriter(bufferWriter))
            {
                element.WriteTo(jsonWriter);
            }

            return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options);
        }
    }
}
