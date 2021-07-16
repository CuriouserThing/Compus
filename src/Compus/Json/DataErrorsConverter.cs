using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Compus.Rest;

namespace Compus.Json
{
    internal class DataErrorsConverter : JsonConverter<IReadOnlyList<DataError>>
    {
        public override IReadOnlyList<DataError> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var stack = new Stack<string>();
            stack.Push("$");

            List<DataError> errors = new();
            while (stack.Count > 0 && reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    stack.Pop();
                }
                else if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? name = reader.GetString();
                    if (name == "_errors")
                    {
                        reader.Read();
                        var converter = (JsonConverter<DataError[]>)options.GetConverter(typeof(DataError[]));
                        DataError[]? currentErrors = converter.Read(ref reader, typeof(DataError[]), options);
                        if (currentErrors is null)
                        {
                            throw new JsonException();
                        }

                        string path = string.Join(string.Empty, stack.Reverse());
                        errors.AddRange(currentErrors.Select(error => new DataError
                        {
                            Code    = error.Code,
                            Message = error.Message,
                            Path    = path,
                        }));
                    }
                    else
                    {
                        string path = int.TryParse(name, out int n) ? $"[{n}]" : $".{name}";
                        stack.Push(path);
                    }
                }
            }

            return errors;
        }

        public override void Write(Utf8JsonWriter writer, IReadOnlyList<DataError> value, JsonSerializerOptions options)
        {
            throw new InvalidOperationException("Can't write data errors.");
        }
    }
}
