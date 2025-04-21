using codecrafters_http_server.src;
using codecrafters_http_server.src.Handlers;
using EasyCompressor;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

public class ClientHandler : IDisposable
{
    private static readonly byte[] DoubleLineBreak = Encoding.UTF8.GetBytes("\r\n\r\n");

    private const int MaxRequestSize = 1024 * 32;
    private const int ReadBufferSize = 1024 * 4;
    private readonly TcpClient _tcpClient;
    private readonly IRequestHandler _requestHandler;

    public ClientHandler(TcpClient tcpClient, IRequestHandler requestHandler)
    {
        _tcpClient = tcpClient;
        _requestHandler = requestHandler;
    }

    public async Task ProcessClientAsync()
    {
        HttpRequest? request = null;
        HttpResponse? response = null;

        try
        {
            request = await GetRequestAsync();
            response = await HandleRequestAsync(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing client: {ex.Message}");
            response = new HttpResponse
            {
                StatusCode = 500,
                StatusText = "Internal Server Error",
                Version = request?.Version ?? "HTTP/1.0"
            };
        }

        try
        {
            await SendResponseAsync(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending response to client: {ex.Message}");
        }
    }

    private static bool HasDoubleLineBreak(Memory<byte> requestData, int totalBytesRead)
        => requestData[..totalBytesRead].Span.IndexOf(DoubleLineBreak) > 0;

    private async Task<HttpRequest> GetRequestAsync()
    {
        var requestData = new Memory<byte>(new byte[MaxRequestSize]);
        var buffer = new Memory<byte>(new byte[ReadBufferSize]);

        int totalBytesRead = 0;
        int bytesRead = 0;
        do
        {
            bytesRead = await _tcpClient.GetStream().ReadAsync(buffer);

            if (bytesRead + totalBytesRead > MaxRequestSize)
            {
                throw new InvalidOperationException($"Request size is bigger than current maximum size ({MaxRequestSize} bytes)");
            }

            buffer[..bytesRead].CopyTo(
                    requestData.Slice(totalBytesRead, bytesRead));
            totalBytesRead += bytesRead;

            if (HasDoubleLineBreak(requestData, totalBytesRead)) break;
        } while (bytesRead > 0);

        return HttpRequest.Parse(requestData[..totalBytesRead]);
    }

    private async Task<HttpResponse> HandleRequestAsync(HttpRequest request)
    {
        var response = await _requestHandler.HandleRequestAsync(request);
        response = await HandleCompressionAsync(request, response);
        return response ?? new HttpResponse
        {
            Version = request.Version,
            StatusCode = 404,
            StatusText = "Not Found"
        };
    }

    private async Task<HttpResponse?> HandleCompressionAsync(HttpRequest request, HttpResponse? response)
    {
        if (response?.Body is not null &&
            request.Headers.TryGetValue("Accept-Encoding", out var encoding) &&
            encoding.Contains("gzip", StringComparison.OrdinalIgnoreCase))
        {
            var compressedResponseBody = await CompressBodyAsync(response.Body.Value);
            response.Headers["Content-Encoding"] = "gzip";
            response.SetBody(compressedResponseBody);
        }

        return response;
    }

    private static async Task<Memory<byte>> CompressBodyAsync(Memory<byte> body)
    {
        await using var outputStream = new MemoryStream();
        var compressor = new GZipCompressor();
        await compressor.CompressAsync(body.ToArray(), outputStream);
        return outputStream.ToArray().AsMemory();
    }

    private async Task SendResponseAsync(HttpResponse response)
    {
        string statusLine = $"{response.Version} {response.StatusCode} {response.StatusText}\r\n";
        await SendLineAsync(statusLine);

        foreach (var header in response.Headers)
        {
            string headerLine = $"{header.Key}: {header.Value}\r\n";
            await SendLineAsync(headerLine);
        }

        // Send empty line to separate headers from body
        await SendLineAsync("\r\n");

        if (response.Body?.IsEmpty == false)
        {
            await SendBytesAsync(response.Body.Value);
        }

        // Send final empty line to indicate end of response
        await SendLineAsync("\r\n\r\n");
    }

    private async Task SendLineAsync(string line)
    {
        await SendBytesAsync(Encoding.UTF8.GetBytes(line).AsMemory());
    }

    private async Task SendBytesAsync(Memory<byte> bytes)
    {
        await _tcpClient.GetStream().WriteAsync(bytes);
    }

    public void Dispose()
    {
        _tcpClient?.Close();
        _tcpClient?.Dispose();
    }
}
