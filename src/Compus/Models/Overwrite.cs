namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#overwrite-object
    /// </summary>
    public record Overwrite
    {
        public enum OverwriteType
        {
            Role = 0,
            Member = 1,
        }

        public Snowflake Id { get; init; }

        public OverwriteType Type { get; init; }

        public PermissionSet Allow { get; init; }

        public PermissionSet Deny { get; init; }
    }
}
