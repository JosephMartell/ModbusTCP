using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusTCP
{
    static class Int16Extensions
    {
        public static byte HighByte(this Int16 i)
        {
            return (byte)(i >> 8);
        }

        public static byte LowByte(this Int16 i)
        {
            return (byte)(i & 0xff);
        }

        public static byte[] GetBytes(this Int16 i)
        {
            return new byte[] { i.HighByte(), i.LowByte() };
        }

        public static Int16 FromBytes(byte high, byte low)
        {
            var temp = (Int16)high;
            temp <<= 8;
            temp += (Int16)low;
            return temp;
        }
    }
}
