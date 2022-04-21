using System;
using System.Collections.Generic;

namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/resources/channel#message-object
/// </summary>
public record Message
{
    public Snowflake Id { get; init; }

    public Snowflake ChannelId { get; init; }

    public Option<Snowflake> GuildId { get; init; }

    public User Author { get; init; }

    public Option<GuildMember> Member { get; init; }

    public string Content { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public DateTimeOffset? EditedTimestamp { get; init; }

    public bool Tts { get; init; }

    public bool MentionEveryone { get; init; }

    public IReadOnlyList<User> Mentions { get; init; }

    public IReadOnlyList<Role> MentionRoles { get; init; }

    public Option<IReadOnlyList<ChannelMention>> MentionChannels { get; init; }

    public IReadOnlyList<Attachment> Attachments { get; init; }

    public IReadOnlyList<Embed> Embeds { get; init; }

    public Option<IReadOnlyList<Reaction>> Reactions { get; init; }

    public Option<string> Nonce { get; init; }

    public bool Pinned { get; init; }

    public Option<Snowflake> WebhookId { get; init; }

    public MessageType Type { get; init; }

    public Option<MessageActivity> Activity { get; init; }

    public Option<Application> Application { get; init; }

    public Option<Snowflake> ApplicationId { get; init; }

    public Option<MessageReference> MessageReference { get; init; }

    public Option<MessageFlags> Flags { get; init; }

    public NullableOption<Message> ReferencedMessage { get; init; }

    public Option<MessageInteraction> Interaction { get; init; }

    public Option<Channel> Thread { get; init; }

    public Option<IReadOnlyList<Component>> Components { get; init; }

    public Option<IReadOnlyList<StickerItem>> StickerItems { get; init; }
}
