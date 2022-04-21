using System;

namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/resources/voice#voice-state-object
/// </summary>
public record VoiceState
{
    public Option<Snowflake> GuildId { get; init; }

    public Snowflake? ChannelId { get; init; }

    public Snowflake UserId { get; init; }

    public Option<GuildMember> Member { get; init; }

    public string SessionId { get; init; }

    public bool Deaf { get; init; }

    public bool Mute { get; init; }

    public bool SelfDeaf { get; init; }

    public bool SelfMute { get; init; }

    public Option<bool> SelfStream { get; init; }

    public bool SelfVideo { get; init; }

    public bool Suppress { get; init; }

    public DateTimeOffset? RequestToSpeakTimestamp { get; init; }
}
