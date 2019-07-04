using System;
using Xunit;
using ModbusTCP;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace ModubusTCPTest
{

    //TODO: Because of recent refactoring, tests should be added to verify the validity of individual message responses
    public class DictMemoryModel
        : IMemoryModel
    {
        Dictionary<int, Int16> _4xMemeory = new Dictionary<int, short>();

        public IEnumerable<short> Read4x(int address, int length)
        {
            short[] values = new short[length];
            for (int i = 0; i < values.Length; i++)
            {
                if (_4xMemeory.ContainsKey(i + address))
                {
                    values[i] = _4xMemeory[i + address];
                }
                else
                {
                    values[i] = 0;
                }
            }
            return values;
        }

        public void Write4x(int address, IEnumerable<short> values)
        {
            int offset = 0;
            values.ToList().ForEach(v => {
                _4xMemeory[address + offset] = v;
                offset++;
            });
        }
    }

    public class ClientTestsFixture
        : IDisposable
    {
        public MBTCPServer MBServer { get; }
        public IMemoryModel MemoryModel = new DictMemoryModel();

        public ClientTestsFixture()
        {
            MBServer = new MBTCPServer(IPAddress.Parse("127.0.0.1"), 502, MemoryModel);
        }

        public void Dispose()
        {
            MBServer.Stop();
        }
    }

    public class ClientTests
        : IClassFixture<ClientTestsFixture>
    {
        private readonly IPAddress testIp = IPAddress.Parse("127.0.0.1");
        ClientTestsFixture serverFixture;

        public ClientTests(ClientTestsFixture fixture)
        {
            serverFixture = fixture;
        }

        [Fact]
        public void Connect_and_disconnect()
        {
            MBTCPClient client = new MBTCPClient(testIp);
            Assert.True(client.Status == ClientStatus.Connected);

            client.Disconnect();
            Assert.True(client.Status == ClientStatus.Disconnected);
        }

        [Fact(DisplayName ="Code 0x03: Read Single Register")]
        public void Read_register()
        {
            MBTCPClient client = new MBTCPClient(testIp);

            serverFixture.MemoryModel.Write4x(1000, new short[] { (Int16)2564 });
            var values = serverFixture.MemoryModel.Read4x(1000, 1);

            var regs = client.ReadHoldingRegisters(1000, 1);
            Assert.Equal(2564, regs.First());
            client.Disconnect();
        }

        [Fact(DisplayName = "Code 0x10: Write Single Register")]
        public void Write_register()
        {
            MBTCPClient client = new MBTCPClient(testIp);
            Int16 testValue = 9999;
            IEnumerable<Int16> writeData = new List<Int16>() { testValue };
            client.WriteMultipleRegisters(1001, writeData);
            client.Disconnect();
            var values = serverFixture.MemoryModel.Read4x(1001, 1);

            Assert.Equal(testValue, values.First());
        }

        [Fact(DisplayName = "Code 0x03: Read multiple registers")]
        public void Read_multiple_registers()
        {
            serverFixture.MemoryModel.Write4x(1000, new short[] { 2564, 9999, 9876 });
            serverFixture.MBServer.RespondWithError = false;
            MBTCPClient client = new MBTCPClient(testIp);
            var regs = client.ReadHoldingRegisters(1000, 3).ToList();
            client.Disconnect();
            Assert.Equal(3, regs.Count);
            Assert.Equal(2564, regs[0]);
            Assert.Equal(9999, regs[1]);
            Assert.Equal(9876, regs[2]);
        }

        [Fact(DisplayName = "Code 0x10: Write multiple registers")]
        public void Write_multiple_registers()
        {
            MBTCPClient client = new MBTCPClient(testIp);
            List<Int16> writeData = new List<Int16>() { 5555, 6666, 7777, 8888 };
            client.WriteMultipleRegisters(1005, writeData);
            client.Disconnect();

            var regs = serverFixture.MemoryModel.Read4x(1005, 4).ToList();

            Assert.Equal(writeData.Count, regs.Count);
            Assert.Equal(writeData[0], regs[0]);
            Assert.Equal(writeData[1], regs[1]);
            Assert.Equal(writeData[2], regs[2]);
            Assert.Equal(writeData[3], regs[3]);
        }

        [Fact(DisplayName = "Bad response from server throws exception.")]
        public void Bad_response_throws_exception()
        {
            serverFixture.MBServer.RespondWithError = true;
            MBTCPClient client = new MBTCPClient(testIp);
            List<Int16> writeData = new List<Int16> { 123, 456, 789, 1234 };
            Assert.ThrowsAny<Exception>(() => client.WriteMultipleRegisters(500, writeData));
        }
    }
}
