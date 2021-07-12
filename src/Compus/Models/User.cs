namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/user#user-object
    /// </summary>
    public record User
    {
        public Snowflake Id { get; init; }

        public string Username { get; init; }

        public ushort Discriminator { get; init; }

        public string? Avatar { get; init; }

        public Option<bool> Bot { get; init; }

        public Option<bool> System { get; init; }

        public Option<bool> MfaEnabled { get; init; }

        public Option<string> Locale { get; init; }

        public Option<bool> Verified { get; init; }

        public NullableOption<string> Email { get; init; }

        public Option<UserFlags> Flags { get; init; }

        public Option<PremiumType> PremiumType { get; init; }

        public Option<UserFlags> PublicFlags { get; init; }
    }
}
