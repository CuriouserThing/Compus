using System.Collections.Generic;
using System.Text.Json.Serialization;
using Compus.Models;

namespace Compus.Rest.Data
{
    public record ApplicationCommandData
    {
        public string Name { get; init; }

        public string Description { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<IReadOnlyList<ApplicationCommandOption>> Options { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Option<bool> DefaultPermission { get; init; }
    }
}
