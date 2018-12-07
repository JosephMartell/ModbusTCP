using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusTCP
{
    //TODO: Provide factory functions that can return an MBAP from length and unit ID OR bytes so return streams can be decoded.
    class MBAP
    {
        private Int16 _length = 0;
        private byte _unitID = 0;
        public MBAP(Int16 length, byte unitID)
        {
            _length = length;
            _unitID = unitID;
        }

        public byte[] GetBytes()
        {
            return new byte[] { 0x00, 0x00, 0x00, 0x00, _length.HighByte(), _length.LowByte(), _unitID };
        }
    }
}
