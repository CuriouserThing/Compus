using System.Collections.Generic;
using System.Text.Json.Serialization;
using Compus.Models;

namespace Compus.Rest.Data;

public record MessageData
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Option<string> Content { get; init; } = null!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Option<bool> Tts { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Option<IReadOnlyList<Embed>> Embeds { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Option<AllowedMentions> AllowedMentions { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Option<MessageReference> MessageReference { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Option<IReadOnlyList<Component>> Components { get; init; }

    public static implicit operator MessageData(string content)
    {
        return new MessageData { Content = content };
    }

    public static implicit operator MessageData(Embed embed)
    {
        return new MessageData { Embeds = new[] { embed } };
    }

    public static implicit operator MessageData(Component component)
    {
        return new MessageData { Components = new[] { component } };
    }
}
