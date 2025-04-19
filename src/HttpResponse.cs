using System.Text;

namespace codecrafters_http_server.src;

public class HttpResponse
{
    public required string Version { get; set; }
    public required int StatusCode { get; set; }
    public required string StatusText { get; set; }
    public Dictionary<string, string> Headers { get; } = new();
    public Memory<byte>? Body { get; private set; }

    public HttpResponse SetBody(string body)
        => SetBody(Encoding.UTF8.GetBytes(body));

    public HttpResponse SetBody(Memory<byte> bytes)
    {
        Body = bytes;
        Headers["Content-Length"] = Body.Value.Length.ToString();
        return this;
    }
}
