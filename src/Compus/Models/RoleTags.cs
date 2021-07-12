using System.Reactive;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/topics/permissions#role-object-role-tags-structure
    /// </summary>
    public record RoleTags
    {
        public Option<Snowflake> BotId { get; init; }

        public Option<Snowflake> IntegrationId { get; init; }

        public NullableOption<Unit> PremiumSubscriber { get; init; }
    }
}
