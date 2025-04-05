using System.Net;
using System.Net.Sockets;
using System.Text;

const int port = 4221;

// Create and start the TCP listener
TcpListener server = new TcpListener(IPAddress.Any, port);
server.Start();
Console.WriteLine($"Server started on port {port}. Press Ctrl+C to stop.");

// Set up cancellation token to handle Ctrl+C
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("Shutting down server...");
    cts.Cancel();
    e.Cancel = true;
};

try
{
    // Accept connections until cancellation is requested
    while (!cts.Token.IsCancellationRequested)
    {
        try
        {
            // Accept a client socket - this will wait for a connection
            var clientSocket = await server.AcceptSocketAsync(cts.Token);

            // Process each client in a separate task
            _ = Task.Run(() => ProcessClientAsync(clientSocket));
        }
        catch (OperationCanceledException oce) when (oce.CancellationToken == cts.Token)
        {
            // This is expected when cancellation is requested
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Socket error while accepting connection: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accepting connection: {ex.Message}");
        }
    }
}
catch (OperationCanceledException)
{
    // This is expected when cancellation is requested
    Console.WriteLine("Server operation was canceled.");
}
finally
{
    // Clean up
    server.Stop();
    Console.WriteLine("Server stopped.");
}

async Task ProcessClientAsync(Socket clientSocket)
{
    try
    {
        using var _ = clientSocket;

        var request = await GetRequestAsync(clientSocket);
        var response = await HandleRequestAsync(request);
        await SendResponseAsync(clientSocket, response);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing client: {ex.Message}");
    }
}

static async Task<string> GetRequestAsync(Socket clientSocket)
{
    var requestBuffer = new byte[2048];
    StringBuilder requestData = new(2048);
    int bytesRead = 0;
    do
    {
        bytesRead = await clientSocket.ReceiveAsync(
            new ArraySegment<byte>(requestBuffer),
            SocketFlags.None);

        requestData.Append(Encoding.UTF8.GetString(requestBuffer, 0, bytesRead));
    } while (bytesRead > 0);

    return requestData.ToString();
}

static async Task<string> HandleRequestAsync(string request)
{
    if (request.StartsWith("GET / "))
    {
        return "HTTP/1.1 200 OK\r\n\r\n";
    }

    return "HTTP/1.1 404 Not Found\r\n\r\n";
}

static async Task SendResponseAsync(Socket clientSocket, string response)
{
    var responseBytes = Encoding.UTF8.GetBytes(response);

    await clientSocket.SendAsync(
        new ArraySegment<byte>(responseBytes),
        SocketFlags.None);
}