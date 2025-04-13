namespace codecrafters_http_server.src.Handlers;

public interface IRequestHandler
{
    ValueTask<HttpResponse?> HandleRequestAsync(HttpRequest request);
}
