using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ModbusTCP
{
    class ReadRegistersMessage
        : ModbusTCPMessage
    {
        public ReadRegistersMessage(MBAP appHeader, ProtocolDataUnit pdu)
            : base(appHeader, pdu)
        { }

        public ReadRegistersMessage(IEnumerable<byte> message)
            : base(message)
        { }

        public override ModbusTCPMessage Process(IMemoryModel memory)
        {
            int address = Int16Extensions.FromBytes(PDU.Data.First(), PDU.Data.Skip(1).First());
            int length = Int16Extensions.FromBytes(PDU.Data.Skip(2).First(), PDU.Data.Skip(3).First());

            try
            {
                var values = memory.Read4x(address, length);
                ProtocolDataUnit pdu = PDU.ValidResponse(
                    new byte[]
                    { (byte)(values.Count() * 2) }
                    .Concat(values.SelectMany(v => v.GetBytes())));
                ModbusTCPMessage reply = Create(
                    MBAP.Create((byte)(pdu.GetBytes().Count() + 1), ApplicationHeader.UnitID),
                    pdu);
                return reply;
            }
            catch
            {
                ProtocolDataUnit pdu = PDU.ErrorResponse(new byte[] { 0x01 });
                ModbusTCPMessage reply = Create(
                    MBAP.Create((byte)(pdu.GetBytes().Count() + 1), ApplicationHeader.UnitID),
                    pdu);
                return reply;
            }
        }
    }
}
