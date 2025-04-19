using System.Text;

namespace codecrafters_http_server.src.Handlers;

public class EchoRequestHandler : IRequestHandler
{
    public ValueTask<HttpResponse?> HandleRequestAsync(HttpRequest request)
    {
        if (request.Method == "GET" && request.Path.StartsWith("/echo/"))
        {
            var result = new HttpResponse
            {
                Version = request.Version,
                StatusCode = 200,
                StatusText = "OK"
            };
            result.SetBody(request.Path.Substring(6));
            result.Headers.Add("Content-Type", "text/plain");

            return ValueTask.FromResult<HttpResponse?>(result);
        }
        return ValueTask.FromResult<HttpResponse?>(null);
    }
}
