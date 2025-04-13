namespace codecrafters_http_server.src.Handlers;

public class HomeRequestHandler : IRequestHandler
{
    public ValueTask<HttpResponse?> HandleRequestAsync(HttpRequest request)
    {
        if (request.Method == "GET" && request.Path == "/")
        {
            return ValueTask.FromResult<HttpResponse?>(new HttpResponse
            {
                Version = request.Version,
                StatusCode = 200,
                StatusText = "OK"
            });
        }
        return ValueTask.FromResult<HttpResponse?>(null);
    }
}
