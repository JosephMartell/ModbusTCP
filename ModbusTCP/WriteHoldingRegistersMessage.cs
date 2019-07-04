using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModbusTCP
{
    class WriteHoldingRegistersMessage
        : ModbusTCPMessage
    {
        public WriteHoldingRegistersMessage(IEnumerable<byte> message)
            : base(message)
        {
        }

        public WriteHoldingRegistersMessage(MBAP appHeader, ProtocolDataUnit pdu)
            : base(appHeader, pdu)
        {
        }

        public override ModbusTCPMessage Process(IMemoryModel memory)
        {
            try
            {
                int address = StartAddress();
                var values = ParseValues();

                memory.Write4x(address, values);

                var pdu = PDU.ValidResponse(((Int16)values.Count()).GetBytes());
                var reply = Create(
                    MBAP.Create((byte)(pdu.GetBytes().Count() + 1), ApplicationHeader.UnitID),
                    pdu);
                return reply;
            }
            catch
            {
                var pdu = PDU.ErrorResponse(new byte[] { 0x01 });
                ModbusTCPMessage reply = Create(
                    MBAP.Create((byte)(pdu.GetBytes().Count() + 1), ApplicationHeader.UnitID),
                    pdu);
                return reply;
            }
        }

        private Int16 StartAddress()
        {
            byte high = PDU.Data.First();
            byte low = PDU.Data.Skip(1).First();
            return Int16Extensions.FromBytes(high, low);
        }

        private IEnumerable<short> ParseValues()
        {
            List<short> values = new List<short>();
            var length = PDU.Data.Skip(4).First();
            var dataBlock = PDU.Data.Skip(5);
            for (int i = 0; i < length / 2; i++)
            {
                values.Add(Int16Extensions.FromBytes(
                    dataBlock.Skip(i * 2).First(),
                    dataBlock.Skip(i * 2 + 1).First()));
            }
            return values;
        }
    }
}
