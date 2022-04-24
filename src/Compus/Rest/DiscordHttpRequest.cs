using System.Net.Http;

namespace Compus.Rest;

public class DiscordHttpRequest
{
    public DiscordHttpRequest(HttpMethod method, string url, params object[] parameters)
    {
        Method = method;
        Url = url;
        Parameters = parameters;
    }

    public HttpMethod Method { get; }

    public string Url { get; }

    public object[] Parameters { get; }

    public HttpContent? Content { get; init; }

    public ResourceScope Scope { get; init; } = new();

    public string GetPath()
    {
        return string.Format(Url, Parameters);
    }
}
