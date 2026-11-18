using System;
using System.Collections.Generic;
using System.Text;
using CLImber;
using ModbusTCP.Client;

namespace CLI_Client
{
    [CommandClass("connect", ShortDescription = "Connect to a ModbusTCP server.")]
    internal class Connect
    {
        [CommandHandler(ShortDescription = "Connect to the server at the provided IP address and default port of 502.")]
        public void ConnectToServer(string ipAddress)
        {
            Console.WriteLine("Connect to server method....");
            MBTCPClient client = new MBTCPClient(System.Net.IPAddress.Parse(ipAddress));

            var response = client.ReadHoldingRegisters(100, 10);
            foreach (var item in response)
            {
                Console.WriteLine(item);
            }
            client.Disconnect();
            Console.WriteLine("Disconnected...");
        }

        [CommandHandler(ShortDescription ="Connect to a server at the provided IP address and port.")]
        public void ConnectToServer(string ipAddress, int port)
        {
            Console.WriteLine("Connect to server method....");
            //ModbusTCP.Client.MBTCPClient client = new ModbusTCP.Client.MBTCPClient(System.Net.IPAddress.Parse(ipAddress), port);
            //Console.WriteLine($"Connected to server at {ipAddress}:{port}");
            //Console.WriteLine($"Client status: {client.Status}");
            //client.Disconnect();
            //Console.WriteLine($"Disconnected from server at {ipAddress}:{port}");
            //Console.WriteLine($"Client status: {client.Status}");
        }

        //TODO: provide interactive menu to query data from the server.
    }
}
