using System.Drawing;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/topics/permissions#role-object
    /// </summary>
    public record Role
    {
        public Snowflake Id { get; init; }

        public string Name { get; init; }

        public Color Color { get; init; }

        public bool Hoist { get; init; }

        public int Position { get; init; }

        public string Permissions { get; init; }

        public bool Managed { get; init; }

        public bool Mentionable { get; init; }

        public Option<RoleTags> Tags { get; init; }
    }
}
