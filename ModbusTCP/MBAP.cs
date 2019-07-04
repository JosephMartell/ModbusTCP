using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModbusTCP
{
    /// <summary>
    /// MODBUS Application Protocol header is used to identify a TCP packet as
    /// a MODBUS message.
    /// </summary>
    class MBAP
    {
        private Int16 _length = 0;
        public Byte UnitID { get; } = 0;
        protected MBAP(Int16 length, byte unitID)
        {
            _length = length;
            UnitID = unitID;
        }

        protected MBAP(IEnumerable<byte> message)
        {
            _length = Int16Extensions.FromBytes(message.Skip(4).First(), message.Skip(5).First());
            UnitID = message.Skip(6).First();
        }

        public byte[] GetBytes()
        {
            return new byte[] { 0x00, 0x00, 0x00, 0x00, _length.HighByte(), _length.LowByte(), UnitID };
        }

        public static MBAP Create(Int16 length, byte unitID)
        {
            return new MBAP(length, unitID);
        }

        public static MBAP ParseFromMessage(IEnumerable<byte> message)
        {
            return new MBAP(message);
        }
    }
}
