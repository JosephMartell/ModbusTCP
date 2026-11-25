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
        : ServerBase,
        IDisposable
    {
        public ServerStatus Status { get; private set; }
        public bool RespondWithError { get; set; }
        private ConnectionListener _listener;
        private readonly Task _receiveDataTask;
        private readonly IMemoryModel _memoryModel;

        public MBTCPServer(IPAddress address, int port, IMemoryModel memoryModel)
            : base(memoryModel)
        {
            _listener = new ConnectionListener(address, port);
            _listener.OnConnectionAccepted += _listener_OnConnectionAccepted;
            _receiveDataTask = _listener.StartAsync();
            _memoryModel = memoryModel;
            Status = ServerStatus.Connected;
        }

        private void _listener_OnConnectionAccepted(object? sender, ConnectionAcceptedEventArgs e)
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
                    ModbusTCPADU request = ModbusTCPADU.Deserialize(buffer.Take(len));

                    PDU responsePDU = PDU.GetInvalidFunctionCodePDU(request.PDU.FunctionCode);
                    
                    if ((request.PDU.IsValidFunctionCode()) && !RespondWithError)
                    {
                        responsePDU = ProcessRequest(request.PDU);
                    }
                    var mtcpResponsADU = ModbusTCPADU.RespondTo(request, responsePDU);
                    e.Client.GetStream().Write(mtcpResponsADU.GetBytes().ToArray(), 0, mtcpResponsADU.GetBytes().Count());
                }
            }, e.CancellationToken);
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