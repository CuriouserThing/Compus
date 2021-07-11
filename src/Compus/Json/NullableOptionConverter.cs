using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Compus.Json
{
    internal class NullableOptionConverter<T> : JsonConverter<NullableOption<T>>
    {
        private readonly JsonConverter<T> _valueConverter;

        public NullableOptionConverter(JsonSerializerOptions options)
        {
            _valueConverter = (JsonConverter<T>)options.GetConverter(typeof(T));
        }

        public override NullableOption<T> Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
        {
            T? value;
            if (reader.TokenType == JsonTokenType.Null)
            {
                value = default(T?);
            }
            else
            {
                value = _valueConverter.Read(ref reader, typeof(T), options);
            }

            return NullableOption<T>.Some(value);
        }

        public override void Write(Utf8JsonWriter writer, NullableOption<T> value, JsonSerializerOptions options)
        {
            if (!value.IsSome(out T? item))
            {
                throw new ArgumentNullException(nameof(value), $"Unable to serialize {NullableOption<T>.None}. Consider using {nameof(JsonIgnoreAttribute)} to avoid this.");
            }

            if (item is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                _valueConverter.Write(writer, item, options);
            }
        }
    }
}
