using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Compus.Json;
using Compus.Models;
using Compus.Rest.Data;
using Microsoft.Extensions.Logging;

namespace Compus.Rest
{
    public class HttpRestClient : IRestClient
    {
        private readonly DiscordHttpClient _client;
        private readonly ILogger _logger;

        public HttpRestClient(DiscordHttpClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        private async Task Request(HttpMethod method, string endpoint, CancellationToken cancellationToken, ResourceScope? scope = null, params object[] parameters)
        {
            var request = new DiscordHttpRequest(method, endpoint, parameters)
            {
                Scope = scope ?? new ResourceScope(),
            };
            using HttpResponseMessage response = await _client.Send(request, cancellationToken);
        }

        private async Task Request<TIn>(HttpMethod method, string endpoint, TIn data, CancellationToken cancellationToken, ResourceScope? scope = null, params object[] parameters)
        {
            using HttpContent content = Serialize(data);
            var request = new DiscordHttpRequest(method, endpoint, parameters)
            {
                Content = content,
                Scope   = scope ?? new ResourceScope(),
            };
            using HttpResponseMessage response = await _client.Send(request, cancellationToken);
        }

        private async Task<TOut> Request<TOut>(HttpMethod method, string endpoint, CancellationToken cancellationToken, ResourceScope? scope = null, params object[] parameters)
        {
            var request = new DiscordHttpRequest(method, endpoint, parameters)
            {
                Scope = scope ?? new ResourceScope(),
            };
            using HttpResponseMessage response = await _client.Send(request, cancellationToken);
            return await Deserialize<TOut>(response);
        }

        private async Task<TOut> Request<TIn, TOut>(HttpMethod method, string endpoint, TIn data, CancellationToken cancellationToken, ResourceScope? scope = null, params object[] parameters)
        {
            using HttpContent content = Serialize(data);
            var request = new DiscordHttpRequest(method, endpoint, parameters)
            {
                Content = content,
                Scope   = scope ?? new ResourceScope(),
            };
            using HttpResponseMessage response = await _client.Send(request, cancellationToken);
            return await Deserialize<TOut>(response);
        }

        private HttpContent Serialize<T>(T data)
        {
            try
            {
                string dataString = JsonSerializer.Serialize(data, JsonOptions.SerializerOptions);
                return new StringContent(dataString, Encoding.UTF8, "application/json");
            }
            catch (JsonException ex)
            {
                _logger.LogError("Couldn't serialize data.", ex);
                throw;
            }
        }

        private async Task<T> Deserialize<T>(HttpResponseMessage response)
        {
            try
            {
                await using Stream content = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<T>(content, JsonOptions.SerializerOptions)
                       ?? throw new JsonException();
            }
            catch (JsonException ex)
            {
                _logger.LogError("Couldn't deserialize response.", ex);
                throw;
            }
        }

        #region Applications

        public async Task<IReadOnlyList<ApplicationCommand>> GetGuildApplicationCommands(Snowflake applicationId, Snowflake guildId, CancellationToken cancellationToken)
        {
            return await Request<IReadOnlyList<ApplicationCommand>>(
                HttpMethod.Get, "/applications/{0}/guilds/{1}/commands",
                cancellationToken,
                new ResourceScope { Application = applicationId, Guild = guildId },
                applicationId, guildId);
        }

        public async Task CreateGuildApplicationCommand(Snowflake applicationId, Snowflake guildId, ApplicationCommandData data, CancellationToken cancellationToken)
        {
            await Request(
                HttpMethod.Post, "/applications/{0}/guilds/{1}/commands", data,
                cancellationToken,
                new ResourceScope { Application = applicationId, Guild = guildId },
                applicationId, guildId);
        }

        public async Task EditApplicationCommandPermissions(Snowflake applicationId, Snowflake guildId, Snowflake commandId, ApplicationCommandPermissionsData data, CancellationToken cancellationToken)
        {
            await Request(
                HttpMethod.Put, "/applications/{0}/guilds/{1}/commands/{2}/permissions", data,
                cancellationToken,
                new ResourceScope { Application = applicationId, Guild = guildId, Command = commandId },
                applicationId, guildId, commandId);
        }

        #endregion

        #region Channels

        public async Task<Channel> GetChannel(Snowflake id, CancellationToken cancellationToken)
        {
            return await Request<Channel>(
                HttpMethod.Get, "/channels/{0}",
                cancellationToken,
                new ResourceScope { Channel = id },
                id);
        }

        public async Task<Message> GetChannelMessage(Snowflake channelId, Snowflake messageId, CancellationToken cancellationToken)
        {
            return await Request<Message>(
                HttpMethod.Get, "/channels/{0}/messages/{1}",
                cancellationToken,
                new ResourceScope { Channel = channelId, Message = messageId },
                channelId, messageId);
        }

        public async Task<Message> CreateMessage(Snowflake channelId, MessageData data, CancellationToken cancellationToken)
        {
            return await Request<MessageData, Message>(
                HttpMethod.Post, "/channels/{0}/messages", data,
                cancellationToken,
                new ResourceScope { Channel = channelId },
                channelId);
        }

        public async Task CreateReaction(Snowflake channelId, Snowflake messageId, string emoji, CancellationToken cancellationToken)
        {
            await Request(
                HttpMethod.Put, "/channels/{0}/messages/{1}/reactions/{2}/@me",
                cancellationToken,
                new ResourceScope { Channel = channelId, Message = messageId },
                channelId, messageId, emoji);
        }

        #endregion

        #region Gateway

        public async Task<BotGateway> GetBotGateway(CancellationToken cancellationToken)
        {
            return await Request<BotGateway>(
                HttpMethod.Get, "/gateway/bot",
                cancellationToken);
        }

        #endregion

        #region Interactions

        public async Task CreateInteractionResponse(Snowflake interactionId, string interactionToken, InteractionResponse response, CancellationToken cancellationToken)
        {
            await Request(
                HttpMethod.Post, "/interactions/{0}/{1}/callback", response,
                cancellationToken,
                new ResourceScope { Interaction = interactionId, InteractionToken = interactionToken },
                interactionId, interactionToken);
        }

        #endregion

        #region Users

        public async Task<User> GetCurrentUser(CancellationToken cancellationToken)
        {
            return await Request<User>(
                HttpMethod.Get, "/users/@me",
                cancellationToken);
        }

        public async Task<User> GetUser(Snowflake id, CancellationToken cancellationToken)
        {
            return await Request<User>(
                HttpMethod.Get, "/users/{0}",
                cancellationToken,
                new ResourceScope { User = id },
                id);
        }

        #endregion
    }
}
