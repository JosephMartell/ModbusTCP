namespace Modbus.Core
{
    public static class WriteMultipleRegistersPDU
    {
        public static WriteMultipleRegistersRequest Request(short startAddress, IEnumerable<Int16> values)
        {
            return new WriteMultipleRegistersRequest(startAddress, values);
        }
        public static WriteMultipleRegistersRequest Request(PDU pdu)
        {
            if (pdu.FunctionCode != ModbusFunctionCode.WriteMultipleRegisters)
            {
                throw new ArgumentException("PDU must be a WriteMultipleRegisters request.");
            }
            if (pdu.GetBytes().Count() < 8)
            {
                throw new ArgumentException($"PDU data must contain at least 8 bytes for a WriteMultipleRegisters request. PDU length: {pdu.GetBytes().Count()}");
            }
            short startAddress = Int16Extensions.FromBytes(pdu.Data[0], pdu.Data[1]);
            short regQuantity = Int16Extensions.FromBytes(pdu.Data[2], pdu.Data[3]);
            short byteCount = pdu.Data[4];
            if (regQuantity < 1 || regQuantity > 123)
            {
                throw new ArgumentOutOfRangeException(nameof(regQuantity), "Quantity must be between 1 and 123.");
            }
            if ((regQuantity * 2) != byteCount)
            {
                throw new ArgumentException($"Byte count does not match quantity of registers to update. Register quantity: {regQuantity}; Byte count: {byteCount}");
            }
            List<short> values = new List<short>();
            for (int i = 0; i < regQuantity; i++)
            {
                short value = Int16Extensions.FromBytes(pdu.Data[5 + (i * 2)], pdu.Data[6 + (i * 2)]);
                values.Add(value);
            }
            return new WriteMultipleRegistersRequest(startAddress, values);
        }

        public class WriteMultipleRegistersRequest
            : PDU
        {
            public short StartAddress { get; set; }
            public IEnumerable<Int16> Values { get; set; }
            internal WriteMultipleRegistersRequest(short startAddress, IEnumerable<Int16> values)
                : base(ModbusFunctionCode.WriteMultipleRegisters)
            {
                StartAddress = startAddress;
                Values = values;
                Int16 regCount = (Int16)values.Count();
                Int16 byteCount = (Int16)(values.Count() * 2);

                Data = new byte[]
                {
                    StartAddress.HighByte(),
                    StartAddress.LowByte(),
                    regCount.HighByte(),
                    regCount.LowByte(),
                    byteCount.LowByte()
                }.Concat(Values.SelectMany(v => new byte[] { v.HighByte(), v.LowByte() })).ToArray();
            }
        }

        public static WriteMultipleRegistersValidResponse ValidResponse(short startAddress, short quantity)
        {
            return new WriteMultipleRegistersValidResponse(startAddress, quantity);
        }

        public class WriteMultipleRegistersValidResponse
            : PDU
        {
            public short StartAddress { get; set; }
            public short Quantity { get; set; }
            internal WriteMultipleRegistersValidResponse(short startAddress, short quantity)
                : base(ModbusFunctionCode.WriteMultipleRegisters)
            {
                StartAddress = startAddress;
                Quantity = quantity;
                Data = new byte[]
                {
                    StartAddress.HighByte(),
                    StartAddress.LowByte(),
                    Quantity.HighByte(),
                    Quantity.LowByte()
                };
            }
        }
    }
}
