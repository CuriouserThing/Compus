using System.Collections.Generic;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/slash-commands#application-command-object
    /// </summary>
    public record ApplicationCommand
    {
        public Snowflake Id { get; init; }

        public Snowflake ApplicationId { get; init; }

        public Option<Snowflake> GuildId { get; init; }

        public string Name { get; init; }

        public string Description { get; init; }

        public Option<IReadOnlyList<ApplicationCommandOption>> Options { get; init; }

        public Option<bool> DefaultPermission { get; init; }
    }
}
