namespace Core.TCPServer;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class TCPServer
{
    private const int BufferSize = 1024;
    private Socket? serverSocket;
    private CancellationTokenSource cts = new CancellationTokenSource();
    private static readonly ConcurrentDictionary<int, Socket> clients = new ConcurrentDictionary<int, Socket>();

    public async Task StartServer()
    {
        try
        {
            serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            
            // Enable dual mode to allow both IPv4 and IPv6 clients
            serverSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            
          //  IPAddress ipv6Address = IPAddress.Parse("2a02:1406:69:6129:3312:3aeb:f05b:43dc");
          //  IPEndPoint localEndPoint = new IPEndPoint(ipv6Address, 13000); // IPv6 loopback address
            serverSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, 13000));
            serverSocket.Listen(10);

            Console.WriteLine("TCP Server started on port 13000...");
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Socket error during initialization: {e.Message}");
            return;
        }

        while (!cts.Token.IsCancellationRequested)
        {
            Console.WriteLine("Waiting for a client...");
            Socket clientSocket = await serverSocket.AcceptAsync();
            int clientHashCode = clientSocket.GetHashCode();
            clients[clientHashCode] = clientSocket;
            Console.WriteLine($"Client connected with ID: {clientHashCode}");

            _ = HandleClientAsync(clientSocket);
        }
    }

    public void StopServer()
    {
        cts.Cancel();

        // Close all client sockets
        foreach (var client in clients.Values)
        {
            client.Close();
        }
        clients.Clear();

        if (serverSocket != null)
        {
            serverSocket.Close();
            serverSocket.Dispose();  // Dispose of the server socket
            Console.WriteLine("TCP Server stopped.");
        }
    }

    private static async Task HandleClientAsync(Socket clientSocket)
    {
        int clientHashCode = clientSocket.GetHashCode();

        try
        {
            byte[] buffer = new byte[BufferSize];

            while (true)
            {
                int bytesRead = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                if (bytesRead == 0) break;

                string receivedHex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", "");
                
                Console.WriteLine($"Received from client {clientHashCode} (Hex): " + receivedHex);

                // ifall att
                string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                /* Uncomment if you want to use the switch-case logic
                switch (receivedMessage.Trim().ToUpper())
                {
                    case "HELLO":
                        string response = "Hello, client!";
                        byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                        await clientSocket.SendAsync(new ArraySegment<byte>(responseBytes), SocketFlags.None);
                        break;

                    // Add more case statements for other commands or messages
                }*/
            }

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            clients.TryRemove(clientHashCode, out _);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Socket error: {e.Message}");
            clients.TryRemove(clientHashCode, out _);
        }
    }
    public static void SendMessageToClient(int clientID, string hexMessage)
    {
        if (clients.TryGetValue(clientID, out Socket? clientSocket))
        {
            byte[] messageBytes = HexStringToByteArray(hexMessage);
            clientSocket.SendAsync(new ArraySegment<byte>(messageBytes), SocketFlags.None);
            Console.WriteLine($"Sent message to client {clientID} (Hex): {hexMessage}");
        }
        else
        {
            Console.WriteLine($"Client {clientID} not found.");
        }
    }

    private static byte[] HexStringToByteArray(string hex)
    {
        int length = hex.Length;
        byte[] bytes = new byte[length / 2];
        for (int i = 0; i < length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }

    public IEnumerable<int> GetAllClientIDs()
    {
        return clients.Keys;
    }   
}