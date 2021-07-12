using System.Collections.Generic;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/application#application-object
    /// </summary>
    public record Application
    {
        public Snowflake Id { get; init; }

        public string Name { get; init; }

        public string? Icon { get; init; }

        public string Description { get; init; }

        public Option<IReadOnlyList<string>> RpcOrigins { get; init; }

        public bool BotPublic { get; init; }

        public bool BotRequireCodeGrant { get; init; }

        public Option<string> TermsOfServiceUrl { get; init; }

        public Option<string> PrivacyPolicyUrl { get; init; }

        public Option<User> Owner { get; init; }

        public string Summary { get; init; }

        public string VerifyKey { get; init; }

        public Team? Team { get; init; }
    }
}
