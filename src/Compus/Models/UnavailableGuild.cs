namespace Compus.Models
{
    /// <summary>
    /// https://discord.com/developers/docs/resources/guild#unavailable-guild-object
    /// </summary>
    public record UnavailableGuild
    {
        public Snowflake Id { get; init; }

        public bool Unavailable { get; init; }
    }
}
