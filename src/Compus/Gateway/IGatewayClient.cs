using System.Threading.Tasks;

namespace Compus.Gateway
{
    public interface IGatewayClient : IGatewayDispatcher
    {
        Task Close();
    }
}
