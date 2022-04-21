using System;
using System.Collections.Generic;

namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/resources/guild#guild-object
/// </summary>
public record Guild
{
    public Snowflake Id { get; init; }

    public string Name { get; init; }

    public string? Icon { get; init; }

    public NullableOption<string> IconHash { get; init; }

    public string? Splash { get; init; }

    public string? DiscoverySplash { get; init; }

    public Option<bool> Owner { get; init; }

    public Snowflake OwnerId { get; init; }

    public Option<string> Permissions { get; init; }

    public NullableOption<string> Region { get; init; }

    public Snowflake? AfkChannelId { get; init; }

    public int AfkTimeout { get; init; }

    public Option<bool> WidgetEnabled { get; init; }

    public NullableOption<Snowflake> WidgetChannelId { get; init; }

    public VerificationLevel VerificationLevel { get; init; }

    public DefaultMessageNotificationLevel DefaultMessageNotifications { get; init; }

    public ExplicitContentFilterLevel ExplicitContentFilter { get; init; }

    public IReadOnlyList<Role> Roles { get; init; }

    public IReadOnlyList<Emoji> Emojis { get; init; }

    public IReadOnlyList<string> Features { get; init; }

    public MfaLevel MfaLevel { get; init; }

    public Snowflake? ApplicationId { get; init; }

    public Snowflake? SystemChannelId { get; init; }

    public SystemChannelFlags SystemChannelFlags { get; init; }

    public Snowflake? RulesChannelId { get; init; }

    public Option<DateTimeOffset> JoinedAt { get; init; }

    public Option<bool> Large { get; init; }

    public Option<bool> Unavailable { get; init; }

    public Option<int> MemberCount { get; init; }

    public Option<IReadOnlyList<VoiceState>> VoiceStates { get; init; }

    public Option<IReadOnlyList<GuildMember>> Members { get; init; }

    public Option<IReadOnlyList<Channel>> Channels { get; init; }

    public Option<IReadOnlyList<Channel>> Threads { get; init; }

    public Option<IReadOnlyList<PresenceUpdate>> Presences { get; init; }

    public NullableOption<int> MaxPresences { get; init; }

    public Option<int> MaxMembers { get; init; }

    public string? VanityUrlCode { get; init; }

    public string? Description { get; init; }

    public string? Banner { get; init; }

    public PremiumTier PremiumTier { get; init; }

    public Option<int> PremiumSubscriptionCount { get; init; }

    public string PreferredLocale { get; init; }

    public Snowflake? PublicUpdatesChannelId { get; init; }

    public Option<int> MaxVideoChannelUsers { get; init; }

    public Option<int> ApproximateMemberCount { get; init; }

    public Option<int> ApproximatePresenceCount { get; init; }

    public Option<WelcomeScreen> WelcomeScreen { get; init; }

    public GuildNsfwLevel NsfwLevel { get; init; }

    public Option<IReadOnlyList<StageInstance>> StageInstances { get; init; }

    public Option<IReadOnlyList<Sticker>> Stickers { get; init; }

    public Option<IReadOnlyList<GuildScheduledEvent>> GuildScheduledEvents { get; init; }

    public bool PremiumProgressBarEnabled { get; init; }
}
