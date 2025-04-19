using System.Text;

namespace codecrafters_http_server.src.Handlers;

public class GetFilesRequestHandler : IRequestHandler
{
    private readonly string _filesDirectory;

    public GetFilesRequestHandler(string filesDirectory)
    {
        _filesDirectory = filesDirectory;
    }

    public ValueTask<HttpResponse?> HandleRequestAsync(HttpRequest request)
    {
        if (request.Method == "GET" && request.Path.StartsWith("/files/"))
        {
            string filePath = Path.Combine(_filesDirectory, request.Path.Substring(7));

            if (File.Exists(filePath))
            {
                HttpResponse result = new()
                {
                    Version = request.Version,
                    StatusCode = 200,
                    StatusText = "OK"
                };
                result.SetBody(File.ReadAllText(filePath));
                result.Headers.Add("Content-Type", "application/octet-stream");

                return ValueTask.FromResult<HttpResponse?>(result);
            }

            HttpResponse notFoundResult = new()
            {
                Version = request.Version,
                StatusCode = 404,
                StatusText = "Not Found"
            };
            notFoundResult.SetBody("File not found.");
            return ValueTask.FromResult<HttpResponse?>(notFoundResult);
        }

        return ValueTask.FromResult<HttpResponse?>(null);
    }
}
