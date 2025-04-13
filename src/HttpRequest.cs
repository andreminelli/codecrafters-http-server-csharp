using System.Collections.ObjectModel;

namespace codecrafters_http_server.src;

public record HttpRequest(string Method, string Path, string Version, ReadOnlyDictionary<string, string> Headers, string Body)
{
    public static HttpRequest Parse(string request)
    {
        var lines = request.Split(["\r\n"], StringSplitOptions.None);

        var (method, path, version) = ParseRequestLine(lines[0]);

        var headers = new Dictionary<string, string>();

        // TODO Parse headers
        // TODO Parse body

        return new HttpRequest(method, path, version, new ReadOnlyDictionary<string, string>(headers), string.Empty);
    }

    private static (string method, string path, string version) ParseRequestLine(string requestLine)
    {
        var parts = requestLine.Split(' ');
        return (method: parts[0], path: parts[1], version: parts[2]);
    }
}