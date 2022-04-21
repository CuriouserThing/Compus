using System.Text.Json;
using System.Text.Json.Serialization;

namespace Compus.Json;

internal static class JsonOptions
{
    private static readonly JsonConverter[] Converters =
    {
        new SnowflakeConverter(),
        new ShardConverter(),
        new ColorConverter(),
        new OptionConverterFactory(true),
        new OptionConverterFactory(false),
        new DataErrorsConverter(),
    };

    public static JsonSerializerOptions SerializerOptions { get; } = CreateSerializerOptions();

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        JsonSerializerOptions options = new()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
        };

        foreach (JsonConverter converter in Converters)
        {
            options.Converters.Add(converter);
        }

        return options;
    }
}
