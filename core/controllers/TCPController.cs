namespace Core.Controllers;

using System;
using System.Threading.Tasks;
using Core.TCPServer;
using System.Collections.Generic;
using System.Linq;

public class TCPController
{   
    public void HandleRequest(string function, int? clientID, string? message)
    {
        switch (function)
        {
            case "start":
                _ = server.StartServer();
                break;
            case "stop":
                StopTCPServer();
                break;
            case "sendmessage":
                if (clientID.HasValue && !string.IsNullOrEmpty(message))
                {
                    SendMessage(clientID.Value, message);
                }
                else
                {
                    Console.WriteLine("Missing clientID or message for 'sendmessage' function.");
                }
                break;
            case "listclientids":
                    GetClientIDs();
                break;
            default:
                Console.WriteLine($"Unknown function: {function}");
                break;
        }
    }



    private readonly TCPServer server = new();

    private void GetClientIDs()
    {
        var clientIDs = server.GetAllClientIDs();
        foreach (var id in clientIDs)
        {
            Console.WriteLine(id);
        }
    }

    private void StopTCPServer()
    {
        server.StopServer();
    }

    private static void SendMessage(int clientID, string message)
    {
        TCPServer.SendMessageToClient(clientID, message);
    }
}