namespace Modbus.Core
{
    public static class UInt16Extensions
    {
        public static byte HighByte(this UInt16 i)
        {
            return (byte)(i >> 8);
        }

        public static byte LowByte(this UInt16 i)
        {
            return (byte)(i & 0xff);
        }

        public static byte[] GetBytes(this UInt16 i)
        {
            return new byte[] { i.HighByte(), i.LowByte() };
        }

        public static UInt16 FromBytes(byte high, byte low)
        {
            var temp = (UInt16)high;
            temp <<= 8;
            temp += (UInt16)low;
            return temp;
        }
    }
}
