namespace codecrafters_http_server.src.Handlers;

public class NotFoundRequestHandler : IRequestHandler
{
    public Task<HttpResponse?> HandleRequestAsync(HttpRequest request)
    {
        return Task.FromResult<HttpResponse?>(new HttpResponse
        {
            Version = request.Version,
            StatusCode = 404,
            StatusText = "Not Found"
        });
    }
}
