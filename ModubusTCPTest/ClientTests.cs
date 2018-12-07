using System;
using Xunit;
using ModbusTCP;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace ModubusTCPTest
{
    public class ClientTests
    {
        private readonly IPAddress testIp = IPAddress.Parse("192.168.217.166");

        [Fact]
        public void Connect_and_disconnect()
        {
            Client client = new Client(testIp);
            Assert.True(client.Status == ClientStatus.Connected);

            client.Disconnect();
            Assert.True(client.Status == ClientStatus.Disconnected);
        }

        [Fact]
        public void Read_register()
        {
            Client client = new Client(testIp);

            var regs = client.ReadHoldingRegisters(1000, 1);
            Assert.Equal(2564, regs.First());
            client.Disconnect();
        }


        [Fact]
        public void Write_register()
        {
            Client client = new Client(testIp);
            Int16 testValue = 9999;
            IEnumerable<Int16> writeData = new List<Int16>() { testValue };
            client.WriteMultipleRegisters(1001, writeData);

            var regs = client.ReadHoldingRegisters(1001, 1);
            Assert.Equal(testValue, regs.First());

            client.Disconnect();
        }

        [Fact]
        public void Read_multiple_registers()
        {
            Client client = new Client(testIp);
            var regs = client.ReadHoldingRegisters(1000, 3).ToList();
            Assert.Equal(3, regs.Count);
            Assert.Equal(2564, regs[0]);
            Assert.Equal(9999, regs[1]);
            Assert.Equal(9876, regs[2]);
        }


        [Fact]
        public void Write_multiple_registers()
        {
            Client client = new Client(testIp);
            List<Int16> writeData = new List<Int16>() { 5555, 6666, 7777, 8888 };
            client.WriteMultipleRegisters(1005, writeData);

            var regs = client.ReadHoldingRegisters(1005, 4).ToList();
            Assert.Equal(writeData.Count, regs.Count);
            Assert.Equal(writeData[0], regs[0]);
            Assert.Equal(writeData[1], regs[1]);
            Assert.Equal(writeData[2], regs[2]);
            Assert.Equal(writeData[3], regs[3]);
        }
    }
}
