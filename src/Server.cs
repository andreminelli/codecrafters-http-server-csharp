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
            var clientSocket = await server.AcceptSocketAsync().ConfigureAwait(false);

            // Process each client in a separate task
            _ = Task.Run(() => ProcessClientAsync(clientSocket));
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

        var requestBuffer = new byte[2048];
        var request = await clientSocket.ReceiveAsync(
            new ArraySegment<byte>(requestBuffer),
            SocketFlags.None);

        // TODO: Parse the request

        byte[] responseBytes = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
        await clientSocket.SendAsync(
            new ArraySegment<byte>(responseBytes),
            SocketFlags.None);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing client: {ex.Message}");
    }
}
