using System;
using Xunit;
using ModbusTCP;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Xunit.v3;
using FluentAssertions;
using ModbusTCP.Protocol;
using ModbusTCP.Server;
using ModbusTCP.Client;
using Modbus.Core;

namespace ModubusTCPTest
{

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
            client.Status.Should().Be(ClientStatus.Connected);

            client.Disconnect();
            client.Status.Should().Be(ClientStatus.Disconnected);
        }

        [Fact(DisplayName ="Code 0x03: Read Single Register")]
        public void Read_register()
        {
            MBTCPClient client = new MBTCPClient(testIp);

            serverFixture.MemoryModel.Write4x(1000, new short[] { (Int16)2564 });
            serverFixture.MBServer.RespondWithError = false;
            var values = serverFixture.MemoryModel.Read4x(1000, 1);

            var regs = client.ReadHoldingRegisters(1000, 1);
            regs.First().Should().Be(2564);
            client.Disconnect();
        }

        [Fact(DisplayName = "Code 0x10: Write Single Register")]
        public void Write_register()
        {
            serverFixture.MBServer.RespondWithError = false;
            MBTCPClient client = new MBTCPClient(testIp);
            Int16 testValue = 9999;
            IEnumerable<Int16> writeData = new List<Int16>() { testValue };
            client.WriteMultipleRegisters(1001, writeData);
            client.Disconnect();
            var values = serverFixture.MemoryModel.Read4x(1001, 1);

            values.First().Should().Be(testValue);
        }

        [Fact(DisplayName = "Code 0x03: Read multiple registers", DisableParallelization = true)]
        public void Read_multiple_registers()
        {
            var testValues = new short[] { 2564, 9999, 9876 };
            serverFixture.MemoryModel.Write4x(1000, testValues);
            serverFixture.MBServer.RespondWithError = false;
            MBTCPClient client = new MBTCPClient(testIp);
            var regs = client.ReadHoldingRegisters(1000, 3).ToList();
            client.Disconnect();
            regs.Should().HaveCount(3).And.ContainInOrder(testValues);
        }

        [Fact(DisplayName = "Code 0x10: Write multiple registers")]
        public void Write_multiple_registers()
        {
            serverFixture.MBServer.RespondWithError = false;
            MBTCPClient client = new MBTCPClient(testIp);
            List<Int16> writeData = new List<Int16>() { 5555, 6666, 7777, 8888 };
            client.WriteMultipleRegisters(1005, writeData);
            client.Disconnect();

            var regs = serverFixture.MemoryModel.Read4x(1005, 4).ToList();
            regs.Should().HaveCount(4).And.ContainInOrder(writeData);
        }

        [Fact(DisplayName = "Bad response from server throws exception.")]
        public void Bad_response_throws_exception()
        {
            serverFixture.MBServer.RespondWithError = true;
            MBTCPClient client = new MBTCPClient(testIp);
            List<Int16> writeData = new List<Int16> { 123, 456, 789, 1234 };
            var action = () => client.WriteMultipleRegisters(500, writeData);
            action.Should().Throw<Exception>("Because the server responded with an error response.");
        }

        [Fact(DisplayName = "Verify memory model")]
        public void Verify_memory_model()
        {
            var testValues = new short[] { 2564, 9999, 9876 };
            serverFixture.MemoryModel.Write4x(1000, testValues);
            var values = serverFixture.MemoryModel.Read4x(1000, 3);
            values.Should().ContainInOrder(testValues);
        }
    }
}
