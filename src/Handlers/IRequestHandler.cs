using System.Threading.Tasks;

namespace codecrafters_http_server.src.Handlers;

public interface IRequestHandler
{
    Task<HttpResponse?> HandleRequestAsync(HttpRequest request);
}
