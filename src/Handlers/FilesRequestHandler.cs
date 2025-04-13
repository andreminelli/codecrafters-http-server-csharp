namespace codecrafters_http_server.src.Handlers;

public class FilesRequestHandler : IRequestHandler
{
    private readonly string _filesDirectory;

    public FilesRequestHandler(string filesDirectory)
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
                result.Body = File.ReadAllText(filePath);
                result.Headers.Add("Content-Type", "application/octet-stream");
                result.Headers.Add("Content-Length", result.Body.Length.ToString());

                return ValueTask.FromResult<HttpResponse?>(result);
            }

            return ValueTask.FromResult<HttpResponse?>(new()
            {
                Version = request.Version,
                StatusCode = 404,
                StatusText = "Not Found",
                Body = "File not found."
            });
        }

        return ValueTask.FromResult<HttpResponse?>(null);
    }
}
