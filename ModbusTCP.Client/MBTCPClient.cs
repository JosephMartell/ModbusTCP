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
        public IEnumerable<bool> ReadCoils(ushort address, ushort count)
        {
            var rcRequest = 
                ModbusTCPADU.Wrap(
                    TransactionID, 
                    ReadCoilsPDU.Request(address, count));

            var response = SendRequest(rcRequest);
            var rcResponse = ReadCoilsPDU.ValidResponse(response.PDU);
            return rcResponse.CoilStatus;

        }

        //0x02 - Read Discrete Inputs
        public void ReadDiscreteInputs(short address, short count)
        {

        }
        //0x03 - Read Holding Registers
        //TODO: Evaluate ReadHoldingRegisters return type.  Should this be words? Bytes? something else?
        //      At the core of the protocol, this is really returning words.  The words can be interpreted
        //      as floating point, integers, unsigned, etc.
        public IEnumerable<Int16> ReadHoldingRegisters(UInt16 startRegister, UInt16 count)
        {
            var rhrRequest = 
                ModbusTCPADU.Wrap(
                    TransactionID, 
                    ReadHoldingRegistersPDU.Request(startRegister, count));
            var response = SendRequest(rhrRequest);
            var rhrResponse = ReadHoldingRegistersPDU.ValidResponse(response.PDU);
            return rhrResponse.Values;

        }

        //0x04 - Read Input Registers
        //0x05 - Write Single Coil
        //0x06 - Write Single Register
        //0x07 - Read Exception Status (Serial only - do not implement)
        //0x08 - Diagnostics (serial only - do not implement)
        //0x0b - Get CommEvent Counter (serial only - do not implement)
        //0x0c - Get Comm Event Log (serial only - do not implement)
        //0x0f - Write Multiple Coils
        //0x10 - Write Multiple Registers
        public void WriteMultipleRegisters(UInt16 startRegister, IEnumerable<Int16> values)
        {
            var mtcpRequest = 
                ModbusTCPADU.Wrap(
                    TransactionID,
                    WriteMultipleRegistersPDU.Request(startRegister, values));

            var response = SendRequest(mtcpRequest);
        }
        //0x11 - Report Server ID (Serial only - do not implement)
        //0x14 - Read File Record
        //0x15 - Write File Record
        //0x16 - Mask Write Register
        //0x17 - Read/Write Multiple Registers
        //0x18 - Read FIFO Queue
        //0x2b - Encapslated Interface Transport (see below)
        //  0x0d CANOpen General Reference Request and Response
        //  0x0e Read Device Identification


        protected ModbusTCPADU SendRequest(ModbusTCPADU request)
        {
            tcpClient.GetStream().Write(request.GetBytes().ToArray(), 0, request.GetBytes().Count());

            byte[] readBuffer = new byte[500];
            int bufferLength = tcpClient.GetStream().Read(readBuffer, 0, 500);

            //TODO: Add error handling for bad length.
            var response = ModbusTCPADU.Deserialize(readBuffer.Take(bufferLength));
            if (response.PDU.IsExceptionResponse())
            {
                throw new Exception("Received exception response.");
            }

            return response;
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
