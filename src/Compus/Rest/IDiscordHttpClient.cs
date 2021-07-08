using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Compus.Rest
{
    public interface IDiscordHttpClient : IDisposable
    {
        Task<HttpResponseMessage> Send(DiscordHttpRequest request)
        {
            return Send(request, CancellationToken.None);
        }

        Task<HttpResponseMessage> Send(DiscordHttpRequest request, CancellationToken cancellationToken);
    }
}
