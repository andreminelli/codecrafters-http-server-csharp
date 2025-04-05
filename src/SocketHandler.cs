using System.Net.Sockets;
using System.Text;

public class SocketHandler : IDisposable
{
    private readonly Socket _clientSocket;

    public SocketHandler(Socket clientSocket)
    {
        _clientSocket = clientSocket;
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
            Console.WriteLine($"Error processing client: {ex.Message}");
        }
    }

    private async Task<string> GetRequestAsync()
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

        return requestData.ToString();
    }

    private async Task<string> HandleRequestAsync(string request)
    {
        if (request.StartsWith("GET / "))
        {
            return "HTTP/1.1 200 OK\r\n\r\n";
        }

        return "HTTP/1.1 404 Not Found\r\n\r\n";
    }

    private async Task SendResponseAsync(string response)
    {
        var responseBytes = Encoding.UTF8.GetBytes(response);

        await _clientSocket.SendAsync(
            new ArraySegment<byte>(responseBytes),
            SocketFlags.None);
    }

    public void Dispose()
    {
        _clientSocket?.Close();
        _clientSocket?.Dispose();
    }
}
