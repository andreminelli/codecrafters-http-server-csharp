namespace codecrafters_http_server.src.Handlers;

public class PipelineRequestHandler : IRequestHandler
{
    private readonly IEnumerable<IRequestHandler> _handlers;

    public PipelineRequestHandler(IEnumerable<IRequestHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task<HttpResponse?> HandleRequestAsync(HttpRequest request)
    {
        foreach (var handler in _handlers)
        {
            var response = await handler.HandleRequestAsync(request);
            if (response != null)
            {
                return response;
            }
        }
        return new HttpResponse
        {
            Version = request.Version,
            StatusCode = 404,
            StatusText = "Not Found"
        };
    }
}
