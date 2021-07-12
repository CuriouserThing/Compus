namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/slash-commands#application-command-permissions-object
    /// </summary>
    public record ApplicationCommandPermissions
    {
        public Snowflake Id { get; init; }

        public ApplicationCommandPermissionType Type { get; init; }

        public bool Permission { get; init; }
    }
}
