using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using ModbusTCP.Protocol;
using Modbus.Core;

namespace ModbusTCP.Client
{
    //TODO: Explicitly handle responses including errors.
    //TODO: Evaluate type of values for WriteMultipleRegisters.  Should this be IEnumerable of bytes? Words? Something else? Should this be overloaded for common formats?
    //TODO: Implement other functions as necessary.
    public class MBTCPClient
        : IDisposable
    {
        public ClientStatus Status { get; private set; }

        public IPAddress ServerIP { get; private set; }
        protected System.Net.Sockets.TcpClient tcpClient;
        private Int16 _transactionID = 0;
        protected Int16 TransactionID
        {
            get
            {
                if (_transactionID >= Int16.MaxValue)
                {
                    _transactionID = 0;
                }
                return _transactionID++;
            }
        }

        public MBTCPClient(IPAddress ip)
        {
            ServerIP = ip;
            Status = ClientStatus.Connected;
            tcpClient = new System.Net.Sockets.TcpClient();
            tcpClient.Connect(new IPEndPoint(ip, 502));
        }

        //0x01 - Read Coils (0x01)
        public IEnumerable<bool> ReadCoils(short address, short count)
        {
            return new List<bool>();
        }

        //0x02 - Read Discrete Inputs
        public void ReadDiscreteInputs(short address, short count)
        {

        }
        //0x03 - Read Holding Registers
        //0x04 - Read Input Registers
        //0x05 - Write Single Coil
        //0x06 - Write Single Register
        //0x07 - Read Exception Status (Serial only - do not implement)
        //0x08 - Diagnostics (serial only - do not implement)
        //0x0b - Get CommEvent Counter (serial only - do not implement)
        //0x0c - Get Comm Event Log (serial only - do not implement)
        //0x0f - Write Multiple Coils
        //0x10 - Write Multiple Registers
        //0x11 - Report Server ID (Serial only - do not implement)
        //0x14 - Read File Record
        //0x15 - Write File Record
        //0x16 - Mask Write Register
        //0x17 - Read/Write Multiple Registers
        //0x18 - Read FIFO Queue
        //0x2b - Encapslated Interface Transport (see below)
        //  0x0d CANOpen General Reference Request and Response
        //  0x0e Read Device Identification

        //TODO: Evaluate ReadHoldingRegisters return type.  Should this be words? Bytes? something else?
        //      At the core of the protocol, this is really returning words.  The words can be interpreted
        //      as floating point, integers, unsigned, etc.
        public IEnumerable<Int16> ReadHoldingRegisters(Int16 startRegister, Int16 count)
        {
            var mtcpRequest = ModbusTCPADU.Wrap(TransactionID, ReadHoldingRegistersPDU.Request(startRegister, count));
            tcpClient.GetStream().Write(mtcpRequest.GetBytes().ToArray() , 0, mtcpRequest.GetBytes().Count());

            byte[] readBuffer = new byte[500];
            int bufferLength = tcpClient.GetStream().Read(readBuffer, 0 , 500);


            //TODO: Add error handling for bad length.
            var mtcpResponse = ModbusTCPADU.Deserialize(readBuffer.Take(bufferLength));
            
            if (mtcpResponse.PDU.IsExceptionResponse())
            {
                throw new Exception("Received exception response.");
            }

            var rhrResponse = ReadHoldingRegistersPDU.ValidResponse(mtcpResponse.PDU);

            return rhrResponse.Values;

        }

        public void WriteMultipleRegisters(Int16 startRegister, IEnumerable<Int16> values)
        {
            var wmrRequest = WriteMultipleRegistersPDU.Request(startRegister, values);
            var mtcpRequest = ModbusTCPADU.Wrap(TransactionID, wmrRequest);
            tcpClient.GetStream().Write(mtcpRequest.GetBytes().ToArray(), 0, mtcpRequest.GetBytes().Count());

            byte[] readBuffer = new byte[500];

            //technically, the operation should be confirmed by examining the response
            int bufferLength = tcpClient.GetStream().Read(readBuffer, 0, readBuffer.Length);
            var mtcpResponse = ModbusTCPADU.Deserialize(readBuffer.Take(bufferLength));

            //error function codes will be: (original function code) | (0x80)
            if (mtcpResponse.PDU.IsExceptionResponse())
            {
                throw new Exception("Received exception response.");
            }
        }

        public void Disconnect()
        {
            tcpClient.Close();
            Status = ClientStatus.Disconnected;
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
