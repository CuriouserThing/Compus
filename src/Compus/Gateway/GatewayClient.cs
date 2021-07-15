using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Compus.Gateway.Events;
using Compus.Json;
using Compus.Models;
using Compus.Rest;
using Microsoft.Extensions.Logging;

namespace Compus.Gateway
{
    public class GatewayClient : IGatewayClient, IAsyncDisposable
    {
        /// <summary>
        ///     Hardcoded gateway version
        /// </summary>
        private const int Version = 9;

        /// <summary>
        ///     Cached gateway endpoint with a safe initial fallback in case of emergency
        /// </summary>
        private static string _wssUrl = "wss://gateway.discord.gg/";

        private readonly Identity _identity;
        private readonly ILogger _logger;
        private readonly IDisposable _readySubscription;
        private readonly Dictionary<string, Action<JsonElement>> _relayMap;
        private readonly IRestClient _restClient;
        private readonly IDisposable _resumedSubscription;
        private readonly SemaphoreSlim _sessionBaton = new(1, 1);

        private int _isRunning;
        private Session _session;
        private string? _sessionId;

        public GatewayClient(Identity identity, IRestClient restClient, ILogger logger)
        {
            _identity   = identity;
            _restClient = restClient;
            _logger     = logger;

            _session = new NewSession(identity, OnEventDispatch, logger);

            _readySubscription   = Ready.Subscribe(OnReady);
            _resumedSubscription = Resumed.Subscribe(OnResumed);
            _relayMap = new Dictionary<string, Action<JsonElement>>
            {
                ["READY"]   = element => Relay(element, Ready),
                ["RESUMED"] = element => Relay(element, Resumed),
                // ["APPLICATION_COMMAND_CREATE"]    = element => Relay(element, ApplicationCommandCreate),
                // ["APPLICATION_COMMAND_UPDATE"]    = element => Relay(element, ApplicationCommandUpdate),
                // ["APPLICATION_COMMAND_DELETE"]    = element => Relay(element, ApplicationCommandDelete),
                // ["CHANNEL_CREATE"]                = element => Relay(element, ChannelCreate),
                // ["CHANNEL_UPDATE"]                = element => Relay(element, ChannelUpdate),
                // ["CHANNEL_DELETE"]                = element => Relay(element, ChannelDelete),
                // ["CHANNEL_PINS_UPDATE"]           = element => Relay(element, ChannelPinsUpdate),
                // ["THREAD_CREATE"]                 = element => Relay(element, ThreadCreate),
                // ["THREAD_UPDATE"]                 = element => Relay(element, ThreadUpdate),
                // ["THREAD_DELETE"]                 = element => Relay(element, ThreadDelete),
                // ["THREAD_LIST_SYNC"]              = element => Relay(element, ThreadListSync),
                // ["THREAD_MEMBER_UPDATE"]          = element => Relay(element, ThreadMemberUpdate),
                // ["THREAD_MEMBERS_UPDATE"]         = element => Relay(element, ThreadMembersUpdate),
                // ["GUILD_CREATE"]                  = element => Relay(element, GuildCreate),
                // ["GUILD_UPDATE"]                  = element => Relay(element, GuildUpdate),
                // ["GUILD_DELETE"]                  = element => Relay(element, GuildDelete),
                // ["GUILD_BAN_ADD"]                 = element => Relay(element, GuildBanAdd),
                // ["GUILD_BAN_REMOVE"]              = element => Relay(element, GuildBanRemove),
                // ["GUILD_EMOJIS_UPDATE"]           = element => Relay(element, GuildEmojisUpdate),
                // ["GUILD_INTEGRATIONS_UPDATE"]     = element => Relay(element, GuildIntegrationsUpdate),
                // ["GUILD_MEMBER_ADD"]              = element => Relay(element, GuildMemberAdd),
                // ["GUILD_MEMBER_REMOVE"]           = element => Relay(element, GuildMemberRemove),
                // ["GUILD_MEMBER_UPDATE"]           = element => Relay(element, GuildMemberUpdate),
                // ["GUILD_MEMBERS_CHUNK"]           = element => Relay(element, GuildMembersChunk),
                // ["GUILD_ROLE_CREATE"]             = element => Relay(element, GuildRoleCreate),
                // ["GUILD_ROLE_UPDATE"]             = element => Relay(element, GuildRoleUpdate),
                // ["GUILD_ROLE_DELETE"]             = element => Relay(element, GuildRoleDelete),
                // ["INTEGRATION_CREATE"]            = element => Relay(element, IntegrationCreate),
                // ["INTEGRATION_UPDATE"]            = element => Relay(element, IntegrationUpdate),
                // ["INTEGRATION_DELETE"]            = element => Relay(element, IntegrationDelete),
                ["INTERACTION_CREATE"] = element => Relay(element, InteractionCreate),
                // ["INVITE_CREATE"]                 = element => Relay(element, InviteCreate),
                // ["INVITE_DELETE"]                 = element => Relay(element, InviteDelete),
                ["MESSAGE_CREATE"]                = element => Relay(element, MessageCreate),
                ["MESSAGE_UPDATE"]                = element => Relay(element, MessageUpdate),
                ["MESSAGE_DELETE"]                = element => Relay(element, MessageDelete),
                ["MESSAGE_DELETE_BULK"]           = element => Relay(element, MessageDeleteBulk),
                ["MESSAGE_REACTION_ADD"]          = element => Relay(element, MessageReactionAdd),
                ["MESSAGE_REACTION_REMOVE"]       = element => Relay(element, MessageReactionRemove),
                ["MESSAGE_REACTION_REMOVE_ALL"]   = element => Relay(element, MessageReactionRemoveAll),
                ["MESSAGE_REACTION_REMOVE_EMOJI"] = element => Relay(element, MessageReactionRemoveEmoji),
                // ["PRESENCE_UPDATE"]               = element => Relay(element, PresenceUpdate),
                // ["TYPING_START"]                  = element => Relay(element, TypingStart),
                // ["USER_UPDATE"]                   = element => Relay(element, UserUpdate),
                // ["VOICE_STATE_UPDATE"]            = element => Relay(element, VoiceStateUpdate),
                // ["VOICE_SERVER_UPDATE"]           = element => Relay(element, VoiceServerUpdate),
                // ["WEBHOOKS_UPDATE"]               = element => Relay(element, WebhooksUpdate),
            };
        }

        public async Task Close()
        {
            await _sessionBaton.WaitAsync();
            try { await _session.Close(); }
            finally { _sessionBaton.Release(); }
        }

        public async Task CloseAndResume()
        {
            await _sessionBaton.WaitAsync();
            try { await _session.CloseAndResume(); }
            finally { _sessionBaton.Release(); }
        }

        public async Task RunAsync()
        {
            if (_isDisposed) { throw new ObjectDisposedException(nameof(GatewayClient)); }

            if (Interlocked.Exchange(ref _isRunning, 1) == 1)
            {
                throw new InvalidOperationException("Another thread is already running this client.");
            }

            try
            {
                while (await RunSession()) { }
            }
            finally
            {
                _isRunning = 0;
            }
        }

        private async Task<bool> RunSession()
        {
            try
            {
                BotGateway gateway = await _restClient.GetBotGateway(CancellationToken.None);
                SessionStartLimit limit = gateway.SessionStartLimit;
                var reset = TimeSpan.FromMilliseconds(limit.ResetAfter);
                string today = limit.ResetAfter == 0 ? "today" : $"over {reset}";
                _logger.LogInformation($"{limit.Remaining}/{limit.Total} sessions remaining {today}.");

                _wssUrl = gateway.Url;
            }
            catch
            {
                _logger.LogWarning($"Couldn't get info on gateway bot connection. Using the cached endpoint {_wssUrl} as a fallback.");
            }

            string endpoint = _wssUrl + $"?v={Version}&encoding=json";
            _logger.LogInformation($"Client connecting to gateway at {endpoint}.");
            await _session.Run(new Uri(endpoint));

            bool reconnect;
            await _sessionBaton.WaitAsync();
            try
            {
                int? seq = _session.Seq;
                reconnect = !_session.CannotReconnect;
                bool resume = !_session.CannotResume;
                await _session.DisposeAsync();

                if (reconnect && resume && _sessionId is not null && seq is not null)
                {
                    var data = new ResumeData
                    {
                        Token     = _identity.Token,
                        SessionId = _sessionId,
                        Seq       = seq.Value,
                    };
                    _session = new ResumedSession(data, OnEventDispatch, _logger);
                }
                else
                {
                    // Create a new session *even if* we're not reconnecting [right now].
                    _sessionId = null;
                    _session   = new NewSession(_identity, OnEventDispatch, _logger);
                }
            }
            finally
            {
                _sessionBaton.Release();
            }

            return reconnect;
        }

        private void OnReady(ReadyEventArgs args)
        {
            _sessionId = args.SessionId;

            // Print some misc. info
            StringBuilder sb = new();

            string username = args.User.Username;
            ushort disc = args.User.Discriminator;
            sb.Append($"Connected as {username}#{disc}");

            sb.Append(args.Shard.IsSome(out Shard? shard) ? $" | Shard {shard}" : " | Shardless");

            int c = args.Guilds.Count;
            string s = c == 1 ? "" : "s";
            sb.Append($" | {c} guild{s}");

            _logger.LogInformation(sb.ToString());
            _logger.LogInformation("Gateway is ready to send events to client.");
        }

        private void OnResumed(Unit _)
        {
            _logger.LogInformation("Gateway has fully resumed a reconnected session.");
        }

        private void OnEventDispatch(string type, JsonElement data)
        {
            if (_relayMap.TryGetValue(type, out var relay))
            {
                try
                {
                    relay(data);
                }
                catch (JsonException)
                {
                    _logger.LogWarning($"Malformed event data in an event of type {type}.");
                    var obs = (MalformedEvent as GatewayObservable<UnknownEventArgs>)!;
                    obs.Relay(new UnknownEventArgs
                    {
                        Type = type,
                        Data = data,
                    }, _logger);
                }
            }
            else
            {
                _logger.LogWarning($"Unknown event type {type}.");
                var obs = (UnknownEvent as GatewayObservable<UnknownEventArgs>)!;
                obs.Relay(new UnknownEventArgs
                {
                    Type = type,
                    Data = data,
                }, _logger);
            }
        }

        private void Relay<T>(JsonElement element, IObservable<T> observable)
        {
            T data = element.ToObject<T>(JsonOptions.SerializerOptions)!;
            var obs = (observable as GatewayObservable<T>)!;
            obs.Relay(data, _logger);
        }

        #region IGatewayClient events

        public IObservable<UnknownEventArgs> UnknownEvent { get; } = new GatewayObservable<UnknownEventArgs>();
        public IObservable<UnknownEventArgs> MalformedEvent { get; } = new GatewayObservable<UnknownEventArgs>();

        public IObservable<ReadyEventArgs> Ready { get; } = new GatewayObservable<ReadyEventArgs>();
        public IObservable<Unit> Resumed { get; } = new GatewayObservable<Unit>();
        public IObservable<Interaction> InteractionCreate { get; } = new GatewayObservable<Interaction>();
        public IObservable<Message> MessageCreate { get; } = new GatewayObservable<Message>();
        public IObservable<Message> MessageUpdate { get; } = new GatewayObservable<Message>();
        public IObservable<MessageDeleteEventArgs> MessageDelete { get; } = new GatewayObservable<MessageDeleteEventArgs>();
        public IObservable<MessageDeleteBulkEventArgs> MessageDeleteBulk { get; } = new GatewayObservable<MessageDeleteBulkEventArgs>();
        public IObservable<MessageReactionAddEventArgs> MessageReactionAdd { get; } = new GatewayObservable<MessageReactionAddEventArgs>();
        public IObservable<MessageReactionRemoveEventArgs> MessageReactionRemove { get; } = new GatewayObservable<MessageReactionRemoveEventArgs>();
        public IObservable<MessageReactionRemoveAllEventArgs> MessageReactionRemoveAll { get; } = new GatewayObservable<MessageReactionRemoveAllEventArgs>();
        public IObservable<MessageReactionRemoveEmojiEventArgs> MessageReactionRemoveEmoji { get; } = new GatewayObservable<MessageReactionRemoveEmojiEventArgs>();

        #endregion

        #region IDisposable

        private bool _isDisposed;

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) { return; }

            _isDisposed = true;

            await _session.DisposeAsync();
            _readySubscription.Dispose();
            _resumedSubscription.Dispose();
        }

        #endregion
    }
}
