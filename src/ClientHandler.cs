using codecrafters_http_server.src;
using codecrafters_http_server.src.Handlers;
using System.Net.Sockets;
using System.Text;

public class ClientHandler : IDisposable
{
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

        await SendResponseAsync(response);
    }

    private async Task<HttpRequest> GetRequestAsync()
    {
        var requestBuffer = new byte[2048];
        StringBuilder requestData = new(2048);
        int bytesRead = 0;
        do
        {
            bytesRead = await _tcpClient.GetStream().ReadAsync(
                new ArraySegment<byte>(requestBuffer));

            requestData.Append(Encoding.UTF8.GetString(requestBuffer, 0, bytesRead));

            if (requestData.ToString().EndsWith("\r\n\r\n")) break;
        } while (bytesRead > 0);

        return HttpRequest.Parse(requestData.ToString());
    }

    private async Task<HttpResponse> HandleRequestAsync(HttpRequest request)
    {
        var response = await _requestHandler.HandleRequestAsync(request);
        return response ?? new HttpResponse
        {
            Version = request.Version,
            StatusCode = 404,
            StatusText = "Not Found"
        };
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

        if (!string.IsNullOrEmpty(response.Body))
        {
            await SendLineAsync(response.Body);
        }

        // Send final empty line to indicate end of response
        await SendLineAsync("\r\n\r\n");
    }

    private async Task SendLineAsync(string line)
    {
        byte[] lineBytes = Encoding.UTF8.GetBytes(line);
        await _tcpClient.GetStream().WriteAsync(
            new ArraySegment<byte>(lineBytes));
    }

    public void Dispose()
    {
        _tcpClient?.Close();
        _tcpClient?.Dispose();
    }
}
