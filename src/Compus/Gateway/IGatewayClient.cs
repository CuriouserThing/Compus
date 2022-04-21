using System.Threading.Tasks;

namespace Compus.Gateway
{
    public interface IGatewayClient : IGatewayDispatcher
    {
        Task Run();

        Task Close();

        Task CloseAndResume();
    }
}
