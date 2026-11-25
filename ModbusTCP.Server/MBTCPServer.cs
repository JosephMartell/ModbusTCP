using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ModbusTCP.Protocol;
using Modbus.Core;
using System.Diagnostics;


namespace ModbusTCP.Server
{


    public enum ServerStatus
    {
        Connected,
        Disconnected
    }


    //TODO: Allow selection of supported Modbus functions at instantiation
    /// <summary>
    /// The MBTCPServer should really consist of 3 parts - the memory model, the connection 
    /// request listener, and the request handler.  The challenge is getting this mental model
    /// into a workable code state.
    /// 
    /// 1st part - the connection handler
    /// I should be able to detect and acknowledge a connection request.  In my current conception of
    /// how this should work this object should kick out an object that represents the connection.
    /// 
    /// 2nd part - the request handler
    /// This object shold respond to a new request from the client.
    /// </summary>
    public class MBTCPServer
        : IDisposable
    {
        public ServerStatus Status { get; private set; }
        public bool RespondWithError { get; set; }
        private ConnectionListener _listener;
        private readonly Task _receiveDataTask;
        private readonly IMemoryModel _memoryModel;

        public MBTCPServer(IPAddress address, int port, IMemoryModel memoryModel)
        {
            _listener = new ConnectionListener(address, port);
            _listener.OnConnectionAccepted += _listener_OnConnectionAccepted;
            _receiveDataTask = _listener.StartAsync();
            _memoryModel = memoryModel;
            Status = ServerStatus.Connected;
        }

        private void _listener_OnConnectionAccepted(object sender, ConnectionAcceptedEventArgs e)
        {
            Task.Run(async () =>
            {
                while (e.Client.Connected)
                {
                    // Maximum Modbus TCP message length is 260 bytes: 7 bytes for the MBAP
                    // and 253 for the maximum PDU length. (ref: Modbus specification)
                    // Limiting message buffer length will help mitigate attacks.
                    byte[] buffer = new byte[260]; 
                    var len = await e.Client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                    ModbusTCPADU message = ModbusTCPADU.Deserialize(buffer.Take(len));


                    //TODO: As soon as we get past the TCP ADU and start handling the PDU, we should pass this off to the ServerBase functions in Modbus.Core.
                    IEnumerable<byte> response = IllegalFunctionResponse(buffer);
                    if ((!message.PDU.IsValidFunctionCode()) || RespondWithError)
                    {
                        response = ModbusTCPADU.RespondTo(message, PDU.GetInvalidFunctionCodePDU(message.PDU.FunctionCode)).GetBytes();                        
                    }
                    else
                    {
                        switch (message.PDU.FunctionCode)
                        {
                            case ModbusFunctionCode.ReadCoils:
                                break;
                            case ModbusFunctionCode.ReadDiscreteInputs:
                                break;
                            case ModbusFunctionCode.ReadHoldingRegisters:
                                var request = ReadHoldingRegistersPDU.Request(message.PDU.GetBytes());
                                var responsePDU = ReadHoldingRegistersPDU.ValidResponse(_memoryModel.Read4x(request.Address, request.Length));
                                response = ModbusTCPADU.RespondTo(message, responsePDU).GetBytes();
                                break;
                            case ModbusFunctionCode.ReadInputRegisters:
                                break;
                            case ModbusFunctionCode.ForceSingleCoil:
                                break;
                            case ModbusFunctionCode.PresetSingleRegister:
                                break;
                            case ModbusFunctionCode.ReadExceptionStatus:
                                break;
                            case ModbusFunctionCode.Diagnostics:
                                break;
                            case ModbusFunctionCode.Program484:
                                break;
                            case ModbusFunctionCode.Poll484:
                                break;
                            case ModbusFunctionCode.FetchCommEventCounter:
                                break;
                            case ModbusFunctionCode.FetchCommEventLog:
                                break;
                            case ModbusFunctionCode.ProgramController:
                                break;
                            case ModbusFunctionCode.PollController:
                                break;
                            case ModbusFunctionCode.ForceMultipleCoils:
                                break;
                            case ModbusFunctionCode.WriteMultipleRegisters:
                                var wmrRequest = WriteMultipleRegistersPDU.Request(message.PDU);
                                var wmrResponsePDU = WriteMultipleRegistersPDU.ValidResponse(wmrRequest.StartAddress, (UInt16)wmrRequest.Values.Count());
                                _memoryModel.Write4x(wmrRequest.StartAddress, wmrRequest.Values);
                                response = ModbusTCPADU.RespondTo(message, wmrResponsePDU).GetBytes();
                                break;
                            case ModbusFunctionCode.ReportSlaveID:
                                break;
                            case ModbusFunctionCode.Program884:
                                break;
                            case ModbusFunctionCode.ResetCommLink:
                                break;
                            case ModbusFunctionCode.ReadGeneralReference:
                                break;
                            case ModbusFunctionCode.WriteGeneralReference:
                                break;
                            case ModbusFunctionCode.MaskWrite4xRegister:
                                break;
                            case ModbusFunctionCode.ReadWrite4xRegister:
                                break;
                            case ModbusFunctionCode.ReadFIFOQueue:
                                break;
                            case ModbusFunctionCode.ErrorMask:
                                break;
                            default:
                                break;
                        }
                    }
                    e.Client.GetStream().Write(response.ToArray(), 0, response.Count());
                }
            }, e.CancellationToken);
        }

        private IEnumerable<byte> IllegalFunctionResponse(IEnumerable<byte> request)
        {
            return new byte[]
            {
            request.ToArray()[0],
            request.ToArray()[1],
            0x00,
            0x00,
            0x00,
            0x03,
            request.ToArray()[6],
            (byte)(request.ToArray()[7] | 0x80),
            0x01
            };
        }

        public void Stop()
        {
            _listener.Stop();
            Status = ServerStatus.Disconnected;
        }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }

}