using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Compus.Json
{
    internal class OptionConverterFactory : JsonConverterFactory
    {
        private readonly Type _converterType;
        private readonly Type _optionType;

        public OptionConverterFactory(bool nullable)
        {
            if (nullable)
            {
                _optionType    = typeof(NullableOption<>);
                _converterType = typeof(NullableOptionConverter<>);
            }
            else
            {
                _optionType    = typeof(Option<>);
                _converterType = typeof(OptionConverter<>);
            }
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType &&
                   typeToConvert.GetGenericTypeDefinition() == _optionType;
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type type = typeToConvert.GetGenericArguments()[0];
            object? converter = Activator.CreateInstance(
                _converterType.MakeGenericType(type),
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new object[] { options },
                null);
            return converter as JsonConverter;
        }
    }
}
