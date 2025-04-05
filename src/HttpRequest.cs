using System.Collections.ObjectModel;

namespace codecrafters_http_server.src;

public record HttpRequest(string Method, string Path, string Version, ReadOnlyDictionary<string, string> Headers, string Body)
{
    public static HttpRequest Parse(string request)
    {
        var lines = request.Split(["\r\n"], StringSplitOptions.None);

        var requestLine = lines[0].Split(' ');
        var method = requestLine[0];
        var path = requestLine[1];
        var version = requestLine[2];

        var headers = new Dictionary<string, string>();

        // TODO Parse headers
        // TODO Parse body

        return new HttpRequest(method, path, version, new ReadOnlyDictionary<string, string>(headers), string.Empty);
    }
}

