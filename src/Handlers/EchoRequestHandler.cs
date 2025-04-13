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
            result.Body = request.Path.Substring(6);
            result.Headers.Add("Content-Type", "text/plain");
            result.Headers.Add("Content-Length", result.Body.Length.ToString());

            return ValueTask.FromResult<HttpResponse?>(result);
        }
        return ValueTask.FromResult<HttpResponse?>(null);
    }
}
