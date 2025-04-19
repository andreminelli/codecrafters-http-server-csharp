using System.Collections.ObjectModel;
using System.Text;

namespace codecrafters_http_server.src;

public record HttpRequest(string Method, string Path, string Version, ReadOnlyDictionary<string, string> Headers, string Body)
{
    private const string LineBreak = "\r\n";

    public static HttpRequest Parse(Memory<byte> requestBytes)
    {
        string request = Encoding.UTF8.GetString(requestBytes.Span);
        var lines = request.Split([LineBreak], StringSplitOptions.None);

        var (method, path, version) = ParseRequestLine(lines[0]);
        var headers = ParseRequestHeaders(lines);
        var body = ParseRequestBody(lines, headers);
        // TODO Parse body

        return new HttpRequest(method, path, version, new ReadOnlyDictionary<string, string>(headers), body);
    }

    private static (string method, string path, string version) ParseRequestLine(string requestLine)
    {
        var parts = requestLine.Split(' ');
        return (method: parts[0], path: parts[1], version: parts[2]);
    }

    private static Dictionary<string, string> ParseRequestHeaders(string[] lines)
    {
        var headers = new Dictionary<string, string>();
        var linePosition = 1;
        for (var i = linePosition; i < lines.Length; i++, linePosition++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) break;

            var headerParts = lines[i].Split(": ");
            headers.Add(headerParts[0], headerParts[1]);
        }

        return headers;
    }

    private static string ParseRequestBody(string[] lines, Dictionary<string, string> headers)
    {
        var body = string.Empty;

        if (lines.Length > headers.Count + 2 &&
            headers.TryGetValue("Content-Length", out var contentLength))
        {
            body = lines[headers.Count + 2];
        }

        return body;
    }
}
