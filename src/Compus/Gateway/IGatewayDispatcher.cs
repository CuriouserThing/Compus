using System;
using System.Reactive;
using Compus.Gateway.Events;
using Compus.Models;

namespace Compus.Gateway
{
    public interface IGatewayDispatcher
    {
        IObservable<UnknownEventArgs> UnknownEvent { get; }

        IObservable<UnknownEventArgs> MalformedEvent { get; }

        #region Events

        /// <summary>
        ///     contains the initial state information
        /// </summary>
        IObservable<ReadyEventArgs> Ready { get; }

        /// <summary>
        ///     response to Resume
        /// </summary>
        IObservable<Unit> Resumed { get; }

        // /// <summary>
        // ///     new Slash Command was created
        // /// </summary>
        // IObservable<> ApplicationCommandCreate { get; }
        //
        // /// <summary>
        // ///     Slash Command was updated
        // /// </summary>
        // IObservable<> ApplicationCommandUpdate { get; }
        //
        // /// <summary>
        // ///     Slash Command was deleted
        // /// </summary>
        // IObservable<> ApplicationCommandDelete { get; }
        //
        // /// <summary>
        // ///     new guild channel created
        // /// </summary>
        // IObservable<> ChannelCreate { get; }
        //
        // /// <summary>
        // ///     channel was updated
        // /// </summary>
        // IObservable<> ChannelUpdate { get; }
        //
        // /// <summary>
        // ///     channel was deleted
        // /// </summary>
        // IObservable<> ChannelDelete { get; }
        //
        // /// <summary>
        // ///     message was pinned or unpinned
        // /// </summary>
        // IObservable<> ChannelPinsUpdate { get; }
        //
        // /// <summary>
        // ///     lazy-load for unavailable guild, guild became available, or user joined a new guild
        // /// </summary>
        // IObservable<> GuildCreate { get; }
        //
        // /// <summary>
        // ///     guild was updated
        // /// </summary>
        // IObservable<> GuildUpdate { get; }
        //
        // /// <summary>
        // ///     guild became unavailable, or user left/was removed from a guild
        // /// </summary>
        // IObservable<> GuildDelete { get; }
        //
        // /// <summary>
        // ///     user was banned from a guild
        // /// </summary>
        // IObservable<> GuildBanAdd { get; }
        //
        // /// <summary>
        // ///     user was unbanned from a guild
        // /// </summary>
        // IObservable<> GuildBanRemove { get; }
        //
        // /// <summary>
        // ///     guild emojis were updated
        // /// </summary>
        // IObservable<> GuildEmojisUpdate { get; }
        //
        // /// <summary>
        // ///     guild integration was updated
        // /// </summary>
        // IObservable<> GuildIntegrationsUpdate { get; }
        //
        // /// <summary>
        // ///     new user joined a guild
        // /// </summary>
        // IObservable<> GuildMemberAdd { get; }
        //
        // /// <summary>
        // ///     user was removed from a guild
        // /// </summary>
        // IObservable<> GuildMemberRemove { get; }
        //
        // /// <summary>
        // ///     guild member was updated
        // /// </summary>
        // IObservable<> GuildMemberUpdate { get; }
        //
        // /// <summary>
        // ///     response to Request Guild Members
        // /// </summary>
        // IObservable<> GuildMembersChunk { get; }
        //
        // /// <summary>
        // ///     guild role was created
        // /// </summary>
        // IObservable<> GuildRoleCreate { get; }
        //
        // /// <summary>
        // ///     guild role was updated
        // /// </summary>
        // IObservable<> GuildRoleUpdate { get; }
        //
        // /// <summary>
        // ///     guild role was deleted
        // /// </summary>
        // IObservable<> GuildRoleDelete { get; }

        /// <summary>
        ///     user used an interaction, such as a Slash Command
        /// </summary>
        IObservable<Interaction> InteractionCreate { get; }

        // /// <summary>
        // ///     invite to a channel was created
        // /// </summary>
        // IObservable<> InviteCreate { get; }
        //
        // /// <summary>
        // ///     invite to a channel was deleted
        // /// </summary>
        // IObservable<> InviteDelete { get; }

        /// <summary>
        ///     message was created
        /// </summary>
        IObservable<Message> MessageCreate { get; }

        /// <summary>
        ///     message was edited
        /// </summary>
        IObservable<Message> MessageUpdate { get; }

        /// <summary>
        ///     message was deleted
        /// </summary>
        IObservable<MessageDeleteEventArgs> MessageDelete { get; }

        /// <summary>
        ///     multiple messages were deleted at once
        /// </summary>
        IObservable<MessageDeleteBulkEventArgs> MessageDeleteBulk { get; }

        /// <summary>
        ///     user reacted to a message
        /// </summary>
        IObservable<MessageReactionAddEventArgs> MessageReactionAdd { get; }

        /// <summary>
        ///     user removed a reaction from a message
        /// </summary>
        IObservable<MessageReactionRemoveEventArgs> MessageReactionRemove { get; }

        /// <summary>
        ///     all reactions were explicitly removed from a message
        /// </summary>
        IObservable<MessageReactionRemoveAllEventArgs> MessageReactionRemoveAll { get; }

        /// <summary>
        ///     all reactions for a given emoji were explicitly removed from a message
        /// </summary>
        IObservable<MessageReactionRemoveEmojiEventArgs> MessageReactionRemoveEmoji { get; }

        // /// <summary>
        // ///     user was updated
        // /// </summary>
        // IObservable<> PresenceUpdate { get; }
        //
        // /// <summary>
        // ///     user started typing in a channel
        // /// </summary>
        // IObservable<> TypingStart { get; }
        //
        // /// <summary>
        // ///     properties about the user changed
        // /// </summary>
        // IObservable<> UserUpdate { get; }
        //
        // /// <summary>
        // ///     someone joined, left, or moved a voice channel
        // /// </summary>
        // IObservable<> VoiceStateUpdate { get; }
        //
        // /// <summary>
        // ///     guild's voice server was updated
        // /// </summary>
        // IObservable<> VoiceServerUpdate { get; }
        //
        // /// <summary>
        // ///     guild channel webhook was created, update, or deleted
        // /// </summary>
        // IObservable<> WebhooksUpdate { get; }

        #endregion
    }
}
