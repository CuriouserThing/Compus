using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Compus.Models;
using Compus.Rest.Data;

namespace Compus.Rest
{
    public interface IRestClient
    {
        #region Applications

        Task<IReadOnlyList<ApplicationCommand>> GetGuildApplicationCommands(Snowflake applicationId, Snowflake guildId, CancellationToken cancellationToken);

        Task CreateGuildApplicationCommand(Snowflake applicationId, Snowflake guildId, ApplicationCommandData data, CancellationToken cancellationToken);

        Task EditApplicationCommandPermissions(Snowflake applicationId, Snowflake guildId, Snowflake commandId, ApplicationCommandPermissionsData data, CancellationToken cancellationToken);

        #endregion

        #region Channels

        Task<Channel> GetChannel(Snowflake id, CancellationToken cancellationToken);

        Task<Message> GetChannelMessage(Snowflake channelId, Snowflake messageId, CancellationToken cancellationToken);

        Task<Message> CreateMessage(Snowflake channelId, MessageData data, CancellationToken cancellationToken);

        Task CreateReaction(Snowflake channelId, Snowflake messageId, string emoji, CancellationToken cancellationToken);

        #endregion

        #region Gateway

        Task<BotGateway> GetBotGateway(CancellationToken cancellationToken);

        #endregion

        #region Interactions

        Task CreateInteractionResponse(Snowflake interactionId, string interactionToken, InteractionResponse response, CancellationToken cancellationToken);

        #endregion

        #region Users

        Task<User> GetCurrentUser(CancellationToken cancellationToken);

        Task<User> GetUser(Snowflake id, CancellationToken cancellationToken);

        #endregion
    }
}
