using System.Linq;

namespace codecrafters_http_server.src.Handlers;

public class UserAgentRequestHandler : IRequestHandler
{
    public ValueTask<HttpResponse?> HandleRequestAsync(HttpRequest request)
    {
        if (request.Method == "GET" && request.Path == ("/user-agent"))
        {
            var result = new HttpResponse
            {
                Version = request.Version,
                StatusCode = 200,
                StatusText = "OK"
            };
            result.SetBody(request.Headers.TryGetValue("User-Agent", out var userAgentValue) ?
                userAgentValue : " ");

            result.Headers.Add("Content-Type", "text/plain");

            return ValueTask.FromResult<HttpResponse?>(result);
        }
        return ValueTask.FromResult<HttpResponse?>(null);
    }
}
