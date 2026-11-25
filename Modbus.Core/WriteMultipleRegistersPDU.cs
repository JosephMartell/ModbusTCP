namespace Modbus.Core
{
    public static class WriteMultipleRegistersPDU
    {
        public static WriteMultipleRegistersRequest Request(UInt16 startAddress, IEnumerable<Int16> values)
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
            UInt16 startAddress = UInt16Extensions.FromBytes(pdu.Data[0], pdu.Data[1]);
            UInt16 regQuantity = UInt16Extensions.FromBytes(pdu.Data[2], pdu.Data[3]);
            UInt16 byteCount = pdu.Data[4];
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
            public UInt16 StartAddress { get; set; }
            public IEnumerable<Int16> Values { get; set; }
            internal WriteMultipleRegistersRequest(UInt16 startAddress, IEnumerable<Int16> values)
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

        public static WriteMultipleRegistersValidResponse ValidResponse(UInt16 startAddress, UInt16 quantity)
        {
            if (quantity > 123)
            {
                throw new ArgumentException("Quantity cannot exceed 123 registers.");
            }
            if (startAddress + quantity > 65536)
            {
                throw new ArgumentException("Starting address and quantity exceeds valid memory address space.");
            }
            return new WriteMultipleRegistersValidResponse(startAddress, quantity);
        }

        public class WriteMultipleRegistersValidResponse
            : PDU
        {
            public UInt16 StartAddress { get; set; }
            public UInt16 Quantity { get; set; }
            internal WriteMultipleRegistersValidResponse(UInt16 startAddress, UInt16 quantity)
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

        public static WriteMultipleRegistersExceptionResponse ExceptionResponse(ExceptionCode exceptionCode)
        {
            return new WriteMultipleRegistersExceptionResponse(exceptionCode);
        }

        public class WriteMultipleRegistersExceptionResponse
            : PDU
        {
            public ExceptionCode ExceptionCode { get; private set; }
            public WriteMultipleRegistersExceptionResponse(ExceptionCode exceptionCode)
                : base(ModbusFunctionCode.ReadHoldingRegisters)
            {
                if (exceptionCode != (ExceptionCode.IllegalFunction |
                                      ExceptionCode.IllegalDataAddress |
                                      ExceptionCode.IllegalDataValue |
                                      ExceptionCode.ServerDeviceFailure))
                {
                    throw new ArgumentOutOfRangeException(nameof(exceptionCode), "Invalid exception code.");
                }
                ExceptionCode = exceptionCode;

                Data = new byte[]
                {
                    (byte)ExceptionCode
                };
            }
        }


    }
}
