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
        protected MBTCPClient Client { get; private set; }


        [CommandHandler(ShortDescription = "Connect to the server at the provided IP address and default port of 502.")]
        public void ConnectToServer(string ipAddress)
        {
            Console.WriteLine("Connect to server ....");
            Client = new MBTCPClient(System.Net.IPAddress.Parse(ipAddress));
            PrintMainMenu();
            //var response = Client.ReadHoldingRegisters(100, 10);
            //foreach (var item in response)
            //{
            //    Console.WriteLine(item);
            //}
            Client.Disconnect();
            Console.WriteLine("Disconnected...");
        }

        [CommandHandler(ShortDescription ="Connect to a server at the provided IP address and port.")]
        public void ConnectToServer(string ipAddress, int port)
        {
            Console.WriteLine("Connect to server....");
            Client = new MBTCPClient(System.Net.IPAddress.Parse(ipAddress));
            PrintMainMenu();
            //ModbusTCP.Client.MBTCPClient client = new ModbusTCP.Client.MBTCPClient(System.Net.IPAddress.Parse(ipAddress), port);
            //Console.WriteLine($"Connected to server at {ipAddress}:{port}");
            //Console.WriteLine($"Client status: {client.Status}");
            //client.Disconnect();
            //Console.WriteLine($"Disconnected from server at {ipAddress}:{port}");
            //Console.WriteLine($"Client status: {client.Status}");
            Client.Disconnect();
            Console.WriteLine("Disconnected...");
        }

        //TODO: provide interactive menu to query data from the server.

        private void PrintMainMenu()
        {
            string userInput = string.Empty;
            int menuOption = -1;
            bool quit = false;
            
            while (!quit)
            {
                Console.Clear();
                Console.WriteLine($"Client status: {Client.Status}");
                Console.WriteLine("Main Menu ");
                Console.WriteLine("  1) Read holding registers       2) Read Coils");
                Console.WriteLine("  Q) Disconnect and quit");
                Console.Write("Select an option: ");

                userInput = Console.ReadLine();

                if (userInput.Trim().ToUpper() == "Q")
                {
                    quit = true;
                }
                else if (int.TryParse(userInput, out menuOption))
                {
                    if (menuOption == 1)
                    {
                        Console.WriteLine("Register to start reading from: ");
                        userInput = Console.ReadLine();
                        if (!UInt16.TryParse(userInput, out ushort startReg))
                        {
                            Console.WriteLine("Invalid register.");
                            break;
                        }
                        Console.WriteLine("Number of registers to read: ");
                        userInput = Console.ReadLine();
                        if (!UInt16.TryParse(userInput, out ushort count))
                        {
                            Console.WriteLine("Invalid count.");
                            break;
                        }
                        var response = Client.ReadHoldingRegisters(startReg, count);

                        Console.WriteLine("Returned values: ");
                        foreach (Int16 value in response)
                        {
                            Console.WriteLine(value);
                        }

                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                    else if (menuOption == 2)
                    {
                        Console.WriteLine("Coil to start reading from: ");
                        userInput = Console.ReadLine();
                        if (!UInt16.TryParse(userInput, out ushort startCoil))
                        {
                            Console.WriteLine("Invalid start coil.");
                            break;
                        }
                        Console.WriteLine("Number of coils to read:");
                        userInput = Console.ReadLine();
                        if (!UInt16.TryParse(userInput, out ushort count))
                        {
                            Console.WriteLine("Invalid coild count.");
                            break;
                        }
                        var response = Client.ReadCoils(startCoil, count).Take(count);

                        Console.WriteLine("Returned values: ");
                        int i = 0;
                        foreach (var item in response)
                        {
                            Console.Write($"{item} ");
                            i++;
                            if (i % 8 == 0)
                            {
                                Console.WriteLine();
                            }
                        }
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }

                }



            }



        }
    }
}
