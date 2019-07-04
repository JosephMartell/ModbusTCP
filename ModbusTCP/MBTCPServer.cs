using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;


namespace ModbusTCP
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
                    byte[] buffer = new byte[1000];
                    var len = await e.Client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                    ModbusTCPMessage message = ModbusTCPMessage.ParseBytes(buffer);

                    IEnumerable<byte> response = IllegalFunctionResponse(buffer);
                    if (!RespondWithError)
                    {
                        response = message.Process(_memoryModel).GetBytes();
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
