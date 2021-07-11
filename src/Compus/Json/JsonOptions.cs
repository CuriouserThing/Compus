using System.Text.Json;
using System.Text.Json.Serialization;

namespace Compus.Json
{
    internal static class JsonOptions
    {
        private static readonly JsonConverter[] Converters =
        {
            new SnowflakeConverter(),
            new ShardConverter(),
            new OptionConverterFactory(true),
            new OptionConverterFactory(false),
        };

        public static JsonSerializerOptions SerializerOptions { get; } = CreateSerializerOptions();

        private static JsonSerializerOptions CreateSerializerOptions()
        {
            JsonSerializerOptions options = new()
            {
                NumberHandling       = JsonNumberHandling.AllowReadingFromString,
                PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
            };

            foreach (var converter in Converters)
            {
                options.Converters.Add(converter);
            }

            return options;
        }
    }
}
