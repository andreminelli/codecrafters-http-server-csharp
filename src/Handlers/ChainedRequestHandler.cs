using System.Collections.Generic;
using System.Threading.Tasks;

namespace codecrafters_http_server.src.Handlers;

public class ChainedRequestHandler : IRequestHandler
{
    private readonly IEnumerable<IRequestHandler> _handlers;

    public ChainedRequestHandler(IEnumerable<IRequestHandler> handlers)
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
        return null;
    }
}
