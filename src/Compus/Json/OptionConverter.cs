using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Compus.Json
{
    internal class OptionConverter<T> : JsonConverter<Option<T>> where T : notnull
    {
        private readonly JsonConverter<T> _valueConverter;

        public OptionConverter(JsonSerializerOptions options)
        {
            _valueConverter = (JsonConverter<T>)options.GetConverter(typeof(T));
        }

        public override Option<T> Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                throw new ArgumentNullException(nameof(reader));
            }
            else
            {
                return _valueConverter.Read(ref reader, typeof(T), options)!;
            }
        }

        public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options)
        {
            if (value.IsSome(out T? item))
            {
                _valueConverter.Write(writer, item, options);
            }
            else
            {
                throw new ArgumentNullException(nameof(value), $"Unable to serialize {Option<T>.None}. Consider using {nameof(JsonIgnoreAttribute)} to avoid this.");
            }
        }
    }
}
