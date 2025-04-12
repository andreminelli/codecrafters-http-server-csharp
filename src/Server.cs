using System.Net;
using System.Net.Sockets;
using codecrafters_http_server.src.Handlers;

const int port = 4221;

TcpListener server = new TcpListener(IPAddress.Any, port);
server.Start();
Console.WriteLine($"Server started on port {port}. Press Ctrl+C to stop.");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("Shutting down server...");
    cts.Cancel();
    e.Cancel = true;
};

var handlers = new IRequestHandler[]
{
    new HomeRequestHandler(),
    new NotFoundRequestHandler()
};
var chainedHandler = new ChainedRequestHandler(handlers);

while (!cts.Token.IsCancellationRequested)
{
    try
    {
        var clientSocket = await server.AcceptSocketAsync(cts.Token);

        _ = Task.Run(() =>
        {
            using var handler = new SocketHandler(clientSocket, chainedHandler);
            return handler.ProcessClientAsync();
        });
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
    finally
    {
        server.Stop();
        Console.WriteLine("Server stopped.");
    }
}
