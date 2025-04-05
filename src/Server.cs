using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var clientSocket = await server.AcceptSocketAsync(); // wait for client

var requestBuffer = new byte[2048];
var request = await clientSocket.ReceiveAsync(new ArraySegment<byte>(requestBuffer), SocketFlags.None);
// TODO Parse the request

byte[] responseBytes = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
await clientSocket.SendAsync(new ArraySegment<byte>(responseBytes), SocketFlags.None);
clientSocket.Close();
