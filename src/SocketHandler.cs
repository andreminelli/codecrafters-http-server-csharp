using codecrafters_http_server.src;
using codecrafters_http_server.src.Handlers;
using System.Net.Sockets;
using System.Text;

public class SocketHandler : IDisposable
{
    private readonly Socket _clientSocket;
    private readonly IRequestHandler _requestHandler;

    public SocketHandler(Socket clientSocket, IRequestHandler requestHandler)
    {
        _clientSocket = clientSocket;
        _requestHandler = requestHandler;
    }

    public async Task ProcessClientAsync()
    {
        try
        {
            var request = await GetRequestAsync();
            var response = await HandleRequestAsync(request);
            await SendResponseAsync(response);
        }
        catch (Exception ex)
        {
            // TODO Handle exceptions
            Console.WriteLine($"Error processing client: {ex.Message}");
        }
    }

    private async Task<HttpRequest> GetRequestAsync()
    {
        var requestBuffer = new byte[2048];
        StringBuilder requestData = new(2048);
        int bytesRead = 0;
        do
        {
            bytesRead = await _clientSocket.ReceiveAsync(
                new ArraySegment<byte>(requestBuffer),
                SocketFlags.None);

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
            StatusCode = 500,
            StatusText = "Internal Server Error"
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
        await _clientSocket.SendAsync(
            new ArraySegment<byte>(lineBytes),
            SocketFlags.None);
    }

    public void Dispose()
    {
        _clientSocket?.Close();
        _clientSocket?.Dispose();
    }
}
