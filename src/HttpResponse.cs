namespace codecrafters_http_server.src;

public class HttpResponse
{
    public required string Version { get; set; }
    public required int StatusCode { get; set; }
    public required string StatusText { get; set; }
    public Dictionary<string, string> Headers { get; } = new();
    public string? Body { get; set; }
}
