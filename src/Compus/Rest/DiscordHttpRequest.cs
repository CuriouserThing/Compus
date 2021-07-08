using System.Net.Http;

namespace Compus.Rest
{
    public class DiscordHttpRequest
    {
        public DiscordHttpRequest(HttpMethod method, string endpoint, params object[] parameters)
        {
            Method     = method;
            Endpoint   = endpoint;
            Parameters = parameters;
        }

        public HttpMethod Method { get; }

        public string Endpoint { get; }

        public object[] Parameters { get; }

        public HttpContent? Content { get; init; }

        public ResourceScope Scope { get; init; } = new();

        public string GetPath()
        {
            return string.Format(Endpoint, Parameters);
        }
    }
}
