using System;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Compus.Json;
using Microsoft.Extensions.Logging;

namespace Compus.Gateway
{
    internal abstract class Session : IAsyncDisposable
    {
        /// <summary>
        ///     This is the max number of bytes Discord allows in a sent payload, but we'll also use it for received payloads.
        /// </summary>
        private const int PayloadBufferSize = 4096;

        private readonly Action<string, JsonElement> _eventDispatchCallback;
        private readonly object _heartbeatLock = new();
        private readonly Timer _heartbeatTimer;
        private readonly ILogger _logger;
        private readonly Memory<byte> _payloadBuffer = new byte[PayloadBufferSize];
        private readonly MemoryStream _payloadStream = new(PayloadBufferSize);
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private readonly ClientWebSocket _socketClient = new();
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        private bool _closeWhenAble;
        private int _heartbeatInterval;
        private int _helloCount;
        private long _lastHeartbeat;
        private long _lastHeartbeatAck;

        protected Session(Action<string, JsonElement> eventDispatchCallback, ILogger logger)
        {
            _eventDispatchCallback = eventDispatchCallback;
            _logger                = logger;
            _heartbeatTimer        = new Timer(async _ => await SendHeartbeat());
        }

        private static CancellationToken CancellationToken => CancellationToken.None;

        public int? Seq { get; private set; }

        public bool CannotReconnect { get; private set; }

        public bool CannotResume { get; private set; }

        public async Task Run(Uri endpoint)
        {
            await _socketClient.ConnectAsync(endpoint, CancellationToken);

            while (true)
            {
                switch (_socketClient.State)
                {
                    case WebSocketState.Open:
                    case WebSocketState.CloseSent:
                        // Continue receiving payload(s) normally.
                        break;
                    case WebSocketState.CloseReceived:
                        _logger.LogWarning("Gateway unexpectedly initiated close handshake. Client will complete handshake and end session.");
                        await Close(true, null);
                        return;
                    case WebSocketState.Closed:
                        _logger.LogInformation("Socket connection is closed. Ending session.");
                        return;
                    case WebSocketState.Connecting:
                        _logger.LogWarning("Socket connection is unexpectedly still connecting, but should be connected. Ending session.");
                        return;
                    case WebSocketState.Aborted:
                        _logger.LogWarning("Socket connection is aborted. Ending session.");
                        return;
                    case WebSocketState.None:
                    default:
                        _logger.LogWarning("Socket connection is in an unknown state. Ending session.");
                        return;
                }

                if (_closeWhenAble)
                {
                    _logger.LogInformation("Client is now sending a close frame deferred from before it connected to the gateway.");
                    await Close(true, null);
                }

                GatewayPayload<JsonElement>? payload = null;
                try
                {
                    payload = await ReceivePayload();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception receiving a payload from the gateway.");
                }

                try
                {
                    if (payload is not null) { await ProcessPayload(payload); }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception processing a payload the gateway sent.");
                }
            }
        }

        public async Task Close()
        {
            CannotReconnect = true;
            CannotResume    = true;
            await Close(true, "Closing per library user request.");
        }

        public async Task CloseAndResume()
        {
            await Close(false, "Closing and resuming per library user request.");
        }

        private async Task<GatewayPayload<JsonElement>?> ReceivePayload()
        {
            ValueWebSocketReceiveResult result = await _socketClient.ReceiveAsync(_payloadBuffer, CancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await CompleteCloseHandshake();
                return null;
            }
            else if (result.EndOfMessage)
            {
                // This is the base case representing the vast majority of payloads.
                return JsonSerializer.Deserialize<GatewayPayload<JsonElement>>(_payloadBuffer[..result.Count].Span, JsonOptions.SerializerOptions);
            }
            else
            {
                // Otherwise, we need to memory-stream data.
                _payloadStream.SetLength(0);
                await _payloadStream.WriteAsync(_payloadBuffer[..result.Count]);
                do
                {
                    result = await _socketClient.ReceiveAsync(_payloadBuffer, CancellationToken);
                    await _payloadStream.WriteAsync(_payloadBuffer[..result.Count]);
                } while (!result.EndOfMessage);

                _payloadStream.Position = 0;
                return await JsonSerializer.DeserializeAsync<GatewayPayload<JsonElement>>(_payloadStream, JsonOptions.SerializerOptions);
            }
        }

        private async Task CompleteCloseHandshake()
        {
            WebSocketState state = _socketClient.State;
            switch (state)
            {
                case WebSocketState.CloseReceived:
                    _logger.LogInformation("Gateway initiated close handshake. Client will complete handshake.");
                    await Close(true, "Completing close handshake.");
                    break;
                case WebSocketState.Closed:
                    _logger.LogInformation("Gateway completed close handshake.");
                    break;
                case WebSocketState.Aborted:
                    _logger.LogWarning("Gateway sent a close frame, but the connection is aborted. Ignoring.");
                    break;
                case WebSocketState.None:
                case WebSocketState.Connecting:
                case WebSocketState.Open:
                case WebSocketState.CloseSent:
                default:
                    _logger.LogError($"Gateway sent a close frame, but the client is in unexpected state {state}. Ignoring.");
                    break;
            }

            var status = (GatewayCloseStatus?)_socketClient.CloseStatus;
            switch (status)
            {
                case GatewayCloseStatus.NormalClosure:
                case GatewayCloseStatus.UnknownError:
                case null:
                    // Can still reconnect if otherwise able to
                    // Can still resume if otherwise able to
                    break;
                case GatewayCloseStatus.InvalidSeq:
                case GatewayCloseStatus.SessionTimedOut:
                    // Can still reconnect if otherwise able to
                    CannotResume = true;
                    break;
                case GatewayCloseStatus.UnknownOpcode:
                case GatewayCloseStatus.DecodeError:
                case GatewayCloseStatus.NotAuthenticated:
                case GatewayCloseStatus.AuthenticationFailed:
                case GatewayCloseStatus.AlreadyAuthenticated:
                case GatewayCloseStatus.RateLimited:
                case GatewayCloseStatus.InvalidShard:
                case GatewayCloseStatus.ShardingRequired:
                case GatewayCloseStatus.InvalidApiVersion:
                case GatewayCloseStatus.InvalidIntents:
                case GatewayCloseStatus.DisallowedIntents:
                default:
                    CannotReconnect = true;
                    CannotResume    = true;
                    break;
            }

            // Thoroughly build a message outlining why the gateway closed.
            StringBuilder sb = new();
            sb.Append("Gateway closed socket connection");

            if (status is null)
            {
                sb.Append(" with unknown code");
            }
            else
            {
                sb.Append($" with code {(int)status}");
                if (Enum.IsDefined(status.Value)) { sb.Append($" ({status.Value})"); }
            }

            string? desc = _socketClient.CloseStatusDescription;
            if (!string.IsNullOrWhiteSpace(desc)) { sb.Append($" and message '{desc}'"); }

            sb.Append('.');

            string message = sb.ToString();
            if (status == GatewayCloseStatus.NormalClosure)
            {
                _logger.LogInformation(message);
            }
            else
            {
                _logger.LogError(message);
            }
        }

        private async Task ProcessPayload(GatewayPayload<JsonElement> payload)
        {
            int? oldSeq = Seq;
            if (payload.S.HasValue)
            {
                Seq = payload.S.Value;
            }

            if (oldSeq.HasValue)
            {
                if (!Seq.HasValue)
                {
                    _logger.LogWarning($"Gateway sent a null seq but last sent a non-null seq {oldSeq}.");
                }
                else if (Seq.Value < oldSeq.Value)
                {
                    _logger.LogWarning($"Gateway sent seq {Seq} but last sent a greater seq {oldSeq}.");
                }
                else if (Seq.Value > oldSeq.Value + 1)
                {
                    _logger.LogWarning($"Gateway sent seq {Seq} but last sent seq {oldSeq}, apparently skipping the range in-between.");
                }
            }

            StringBuilder sb = new();
            if (Seq.HasValue && (!oldSeq.HasValue || Seq.Value != oldSeq.Value))
            {
                sb.Append($"{Seq} ");
            }

            sb.Append($"Received {payload.Op}");
            if (payload.T is not null)
            {
                sb.Append($" {payload.T}");
            }

            sb.Append('.');
            _logger.LogDebug(sb.ToString());

            switch (payload.Op)
            {
                case GatewayOpcode.Dispatch:
                    OnGatewayDispatch(payload);
                    break;
                case GatewayOpcode.Heartbeat:
                    await SendHeartbeat();
                    break;
                case GatewayOpcode.Reconnect:
                    await OnReconnect();
                    break;
                case GatewayOpcode.InvalidSession:
                    await OnInvalidSession(payload);
                    break;
                case GatewayOpcode.Hello:
                    await OnGatewayHello(payload);
                    break;
                case GatewayOpcode.HeartbeatAck:
                    OnHeartbeatAck();
                    break;
                case GatewayOpcode.Identify:
                case GatewayOpcode.PresenceUpdate:
                case GatewayOpcode.VoiceStateUpdate:
                case GatewayOpcode.Resume:
                case GatewayOpcode.RequestGuildMembers:
                    _logger.LogError($"Received send-only gateway opcode {(int)payload.Op}. Ignoring.");
                    break;
                default:
                    _logger.LogError($"Received unknown gateway opcode {(int)payload.Op}. Ignoring.");
                    break;
            }
        }

        private async Task OnGatewayHello(GatewayPayload<JsonElement> payload)
        {
            _logger.LogInformation("Gateway said hello! Responding accordingly.");
            _helloCount++;
            if (_helloCount > 1)
            {
                _logger.LogWarning($"Gateway sent Hello event #{_helloCount}. Only one Hello is expected. Client will respond to the Hello normally, but this may ultimately close the connection.");
            }

            HelloData? hello = payload.D.ToObject<HelloData>(JsonOptions.SerializerOptions);
            if (hello is null)
            {
                await Close(false, "No data received in the Hello payload.");
                return;
            }

            await RespondToHello();

            _heartbeatInterval = hello.HeartbeatInterval;
            _logger.LogInformation($"Starting the heartbeat timer at an interval of {_heartbeatInterval}ms.");
            var period = TimeSpan.FromMilliseconds(_heartbeatInterval);
            TimeSpan dueTime = period * 0.5; // the due time shouldn't matter much
            _heartbeatTimer.Change(dueTime, period);
        }

        protected abstract Task RespondToHello();

        private void OnGatewayDispatch(GatewayPayload<JsonElement> payload)
        {
            if (payload.T is null)
            {
                _logger.LogError("Gateway dispatched an event without a t field. Ignoring.");
            }
            else
            {
                _eventDispatchCallback(payload.T, payload.D);
            }
        }

        private async Task OnReconnect()
        {
            _logger.LogInformation("Gateway is requesting a reconnection. Client will disconnect and then attempt to reconnect and resume.");
            await Close(true, "Reconnecting per Opcode 7 Reconnect.");
        }

        private async Task OnInvalidSession(GatewayPayload<JsonElement> payload)
        {
            // There's nothing in documentation to suggest d could be null here, but it's logical for us to allow for it.
            bool? resumable = payload.D.ToObject<bool?>();
            if (!resumable.HasValue || !resumable.Value) { CannotResume = true; }

            _logger.LogInformation("Gateway has invalidated the session. Client will disconnect and then attempt to reconnect.");
            await Close(true, "Reconnecting per Opcode 9 Invalid Session.");
        }

        private void OnHeartbeatAck()
        {
            lock (_heartbeatLock)
            {
                _lastHeartbeatAck = _stopwatch.ElapsedMilliseconds;
            }
        }

        private async Task SendHeartbeat()
        {
            bool sent = await SendPayload(GatewayOpcode.Heartbeat, Seq);
            if (!sent)
            {
                return;
            }

            var tryClose = false;
            lock (_heartbeatLock)
            {
                long thisHeartbeat = _stopwatch.ElapsedMilliseconds;
                long sinceLastAck = thisHeartbeat - _lastHeartbeatAck;
                if (_lastHeartbeatAck < _lastHeartbeat && sinceLastAck > _heartbeatInterval)
                {
                    _logger.LogError($"Client just sent a heartbeat, but the gateway never ACKed the prior one. Additionally, {sinceLastAck}ms has passed since the last ACK and the heartbeat interval is only {_heartbeatInterval}ms. Since the connection may have failed, the client will disconnect and then attempt to reconnect and resume.");
                    tryClose = true;
                }

                _lastHeartbeat = thisHeartbeat;
            }

            if (tryClose)
            {
                await Close(false, "ACK not received between heartbeats; attempting to resume.");
            }
        }

        protected async Task<bool> SendPayload<T>(GatewayOpcode op, T data)
        {
            await _sendLock.WaitAsync();
            try
            {
                if (_socketClient.State != WebSocketState.Open)
                {
                    _logger.LogInformation($"A thread is attempting to send op {op} over the socket connection, but the socket is in non-open state {_socketClient.State}. Returning without sending data.");
                    return false;
                }

                var payload = new GatewayPayload<T>
                {
                    Op = op,
                    D  = data,
                };

                byte[] bytes;
                try
                {
                    bytes = JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions.SerializerOptions);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Could not serialize data for op {op}. Returning without sending data.");
                    return false;
                }

                try
                {
                    var segment = new ArraySegment<byte>(bytes);
                    while (segment.Count > PayloadBufferSize)
                    {
                        await _socketClient.SendAsync(segment[..PayloadBufferSize], WebSocketMessageType.Text, false, CancellationToken);
                        segment = segment[PayloadBufferSize..];
                    }

                    await _socketClient.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Client could not send data for op {op} without error. It's possible the client sent partial data.");
                    return false;
                }
            }
            finally
            {
                _sendLock.Release();
            }

            _logger.LogDebug($"Sent {op}.");
            return true;
        }

        private async Task Close(bool normalClosure, string? reason)
        {
            await _sendLock.WaitAsync();
            try
            {
                WebSocketCloseStatus closeState = normalClosure ? WebSocketCloseStatus.NormalClosure : (WebSocketCloseStatus)4000;
                switch (_socketClient.State)
                {
                    case WebSocketState.Open:
                        await _socketClient.CloseOutputAsync(closeState, reason, CancellationToken);
                        _logger.LogInformation("Client has initiated close handshake.");
                        break;
                    case WebSocketState.CloseReceived:
                        await _socketClient.CloseOutputAsync(closeState, reason, CancellationToken);
                        _logger.LogInformation("Client has completed close handshake.");
                        break;
                    case WebSocketState.Connecting:
                        _closeWhenAble = true;
                        _logger.LogInformation("Client is attempting to send a close frame, but isn't connected yet. Will close when able to.");
                        break;
                    case WebSocketState.CloseSent:
                    case WebSocketState.Closed:
                        _logger.LogInformation("Client is attempting to send a close frame, but already has (possibly from another thread). Ignoring.");
                        break;
                    case WebSocketState.Aborted:
                        _logger.LogInformation("Client is attempting to send a close frame, but is in an aborted state. Ignoring.");
                        break;
                    case WebSocketState.None:
                    default:
                        _logger.LogInformation("Client is attempting to send a close frame, but is in an unknown state. Ignoring.");
                        break;
                }
            }
            finally
            {
                _sendLock.Release();
            }
        }

        #region IDisposable

        private bool _isDisposed;

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) { return; }

            _isDisposed = true;

            _socketClient.Dispose();
            await _heartbeatTimer.DisposeAsync();
            await _payloadStream.DisposeAsync();
            _sendLock.Dispose();
        }

        #endregion
    }
}
