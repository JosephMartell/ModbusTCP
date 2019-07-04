using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using ModbusTCP;
using System.Net;
using System.Threading.Tasks;

namespace ModubusTCPTest
{
    public class ServerTest
    {
        [Fact]
        public void Server_connects_and_disconnects()
        {
            MBTCPServer server = new MBTCPServer(IPAddress.Any, 502, new DictMemoryModel());
            Assert.True(server.Status == ServerStatus.Connected);

            server.Stop();
            Assert.True(server.Status == ServerStatus.Disconnected);
        }

        [Fact]
        public void Server_responds_to_client_connection_request()
        {
            MBTCPServer server = new MBTCPServer(IPAddress.Any, 502, new DictMemoryModel());
            MBTCPClient client = new MBTCPClient(IPAddress.Parse("127.0.0.1"));

            Assert.True(client.Status == ClientStatus.Connected);
            client.Disconnect();
            server.Stop();

            Assert.True(client.Status == ClientStatus.Disconnected);
        }
    }
}
