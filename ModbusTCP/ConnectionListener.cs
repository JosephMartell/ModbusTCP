using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusTCP
{
    class ConnectionAcceptedEventArgs
    : EventArgs
    {
        public TcpClient Client { get; }
        public CancellationToken CancellationToken { get; }

        public ConnectionAcceptedEventArgs(TcpClient client, CancellationToken token)
        {
            Client = client;
            CancellationToken = token;
        }
    }


    class ConnectionListener
        : IDisposable
    {
        private readonly TcpListener _listener;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;

        public event EventHandler<ConnectionAcceptedEventArgs> OnConnectionAccepted;

        public ConnectionListener(IPAddress address, int port)
        {
            _listener = new TcpListener(address, port);
        }

        public bool Listening { get; private set; }

        public async Task StartAsync(CancellationToken? token = null)
        {
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token ?? new CancellationToken());
            _token = _tokenSource.Token;
            _listener.Start();
            Listening = true;

            try
            {
                await Task.Run(async () =>
                {
                    while(!_token.IsCancellationRequested)
                    {
                        var client = await _listener.AcceptTcpClientAsync();
                        OnConnectionAccepted?.Invoke(this, new ConnectionAcceptedEventArgs(client, _token));
                    }
                }, _token);
            }
            finally
            {
                _listener.Stop();
                Listening = false;
            }
        }

        public void Stop()
        {
            _tokenSource?.Cancel();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
