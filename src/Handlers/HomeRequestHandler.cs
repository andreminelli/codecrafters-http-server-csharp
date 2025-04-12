namespace codecrafters_http_server.src.Handlers;

public class HomeRequestHandler : IRequestHandler
{
    public Task<HttpResponse?> HandleRequestAsync(HttpRequest request)
    {
        if (request.Method == "GET" && request.Path == "/")
        {
            return Task.FromResult<HttpResponse?>(new HttpResponse
            {
                Version = request.Version,
                StatusCode = 200,
                StatusText = "OK"
            });
        }
        return Task.FromResult<HttpResponse?>(null);
    }
}
