namespace codecrafters_http_server.src.Handlers;

public class PostFilesRequestHandler : IRequestHandler
{
    private readonly string _filesDirectory;

    public PostFilesRequestHandler(string filesDirectory)
    {
        _filesDirectory = filesDirectory;
    }

    public async ValueTask<HttpResponse?> HandleRequestAsync(HttpRequest request)
    {
        HttpResponse? result = null;

        if (request.Method == "POST" && request.Path.StartsWith("/files/"))
        {
            string filePath = Path.Combine(_filesDirectory, request.Path.Substring(7));

            await File.WriteAllTextAsync(filePath, request.Body);
            result = new()
            {
                Version = request.Version,
                StatusCode = 201,
                StatusText = "Created"
            };
        }

        return result;
    }
}
