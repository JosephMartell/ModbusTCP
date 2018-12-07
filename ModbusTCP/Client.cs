using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace ModbusTCP
{
    //TODO: Implement IDisposable
    //TODO: Explicitly handle responses including errors.
    //TODO: Evaluate ReadHoldingRegisters return type.  Should this be words? Bytes? something else?
    //TODO: Evaluate type of values for WriteMultipleRegisters.  Should this be IEnumerable of bytes? Words? Something else? Should this be overloaded for common formats?
    //TODO: Implement other functions as necessary.
    public class Client
    {
        public ClientStatus Status { get; private set; }
        public System.Net.Sockets.TcpClient tcpClient;
    
        public Client(IPAddress ip)
        {
            Status = ClientStatus.Connected;
            tcpClient = new System.Net.Sockets.TcpClient();
            tcpClient.Connect(new IPEndPoint(ip, 502));
        }

        public IEnumerable<Int16> ReadHoldingRegisters(Int16 startRegister, Int16 count)
        {
            List<byte> mbPacket = new List<byte>();

            mbPacket.Add(0x03);
            mbPacket.AddRange(startRegister.GetBytes());
            mbPacket.AddRange(count.GetBytes());

            MBAP mbap = new MBAP((Int16)(mbPacket.Count + 1), 0);
            mbPacket.InsertRange(0, mbap.GetBytes());

            tcpClient.GetStream().Write(mbPacket.ToArray(), 0, mbPacket.Count);

            byte[] readBuffer = new byte[500];

            tcpClient.GetStream().Read(readBuffer, 0, 500);

            int returnCount = (int)readBuffer[8];

            List<Int16> returnList = new List<short>();
            for (int i = 9; i < 9 + returnCount; i += 2)
            {
                returnList.Add(Int16Extensions.FromBytes(readBuffer[i], readBuffer[i + 1]));
            }

            return returnList;
        }

        public void WriteMultipleRegisters(Int16 startRegister, IEnumerable<Int16> values)
        {
            List<byte> mbPacket = new List<byte>();
            mbPacket.Add(0x10);
            mbPacket.AddRange(startRegister.GetBytes());
            mbPacket.AddRange(((Int16)(values.Count())).GetBytes());

            var data = values.SelectMany((v, r) => new List<byte>() { v.HighByte(), v.LowByte() });
            mbPacket.Add(((Int16)data.Count()).LowByte());
            mbPacket.AddRange(data.ToArray());

            MBAP mbap = new MBAP((Int16)(mbPacket.Count + 1), 0);
            mbPacket.InsertRange(0, mbap.GetBytes());

            tcpClient.GetStream().Write(mbPacket.ToArray(), 0, mbPacket.Count);

            byte[] buffer = new byte[500];

            //technically, the operation should be confirmed by examining the response
            tcpClient.GetStream().Read(buffer, 0, buffer.Length);
        }

        public void Disconnect()
        {
            tcpClient.Close();
            Status = ClientStatus.Disconnected;
        }
    }
}
