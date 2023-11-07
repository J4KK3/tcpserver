namespace Core;
using System;
using Core.Controllers;

class ConsoleHandler
{        
    private TCPController controller = new TCPController();
    private bool start = false;
    private bool running = true;

    public void Start()
    {
        Console.WriteLine("TCP Controller Interactive");
        Console.WriteLine("Commands: start, stop, listclientids, sendmessage [clientID] [message (Hex)]");

        while (running)
        {
            while (!start)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                string[] parts = input.Split(' ');
                string command = parts[0];

                if(command.ToLower() == "start")
                {   
                    start = true;
                    controller.HandleRequest("start", null, null);
                }
            }

            while (start)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                string[] parts = input.Split(' ');
                string command = parts[0];

                switch (command.ToLower())
                {
                    case "stop":
                        controller.HandleRequest("stop", null, null);
                        start = false;
                        break;

                    case "sendmessage":
                        if (parts.Length < 3)
                        {
                            Console.WriteLine("Usage: sendmessage [clientID] [message (Hex)]");
                            break;
                        }

                        if (int.TryParse(parts[1], out int clientID))
                        {
                            string message = string.Join(' ', parts, 2, parts.Length - 2);
                            controller.HandleRequest("sendmessage", clientID, message);
                        }
                        else
                        {
                            Console.WriteLine("Invalid clientID.");
                        }
                        break;

                    case "listclientids":
                        controller.HandleRequest("listclientids", null, null);
                        break;

                    case "exit":
                        start = false;    // Stop the inner loop
                        running = false;  // Stop the outer loop
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        break;
                }
            }
        }
    }
}