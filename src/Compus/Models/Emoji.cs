using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/emoji#emoji-object
    /// </summary>
    public record Emoji
    {
        public Snowflake? Id { get; init; }

        public string? Name { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<IReadOnlyList<Role>> Roles { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<User> User { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<bool> RequireColons { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<bool> Managed { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<bool> Animated { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<bool> Available { get; init; }
    }
}
