using System;
using System.Collections.Generic;
using System.Text;
using CLImber;
using Modbus.Core;

namespace CLI_Client
{
    [CommandClass("local")]
    internal class LocalTest
    {
        [CommandHandler()]
        public void RunTest()
        {
            var req = ReadHoldingRegistersPDU.Request(1000, 10);
            Console.WriteLine("Request:");
            foreach (var b in req.GetBytes())
            {
                Console.WriteLine(b);
            }

            var response = ReadHoldingRegistersPDU.ValidResponse(new short[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            Console.WriteLine("Response:");
            foreach (var b in response.GetBytes())
            {
                Console.WriteLine(b);
            }
        }
    }
}
