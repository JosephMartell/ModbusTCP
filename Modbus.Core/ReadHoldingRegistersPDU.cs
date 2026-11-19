namespace Modbus.Core
{

    public static class ReadHoldingRegistersPDU
    {
        public static ReadHoldingRegistersRequest Request(Int16 address, Int16 length)
        {
            return new ReadHoldingRegistersRequest(address, length);
        }

        public static ReadHoldingRegistersRequest Request(IEnumerable<byte> requestBytes)
        {
            if (requestBytes.Count() != 5)
            {
                throw new ArgumentException("Invalid request bytes for ReadHoldingRegisters.");
            }

            Int16 address = Int16Extensions.FromBytes(requestBytes.Skip(1).First(), requestBytes.Skip(2).First());
            Int16 length = Int16Extensions.FromBytes(requestBytes.Skip(3).First(), requestBytes.Skip(4).First());
            return new ReadHoldingRegistersRequest(address, length);
        }

        public class ReadHoldingRegistersRequest
            : PDU
        {
            public short Address { get; private set; }
            public short Length { get; private set; }
            public ReadHoldingRegistersRequest(short address, short length)
                : base(ModbusFunctionCode.ReadHoldingRegisters)
            {
                Address = address;
                if (length < 1 || length > 125)
                {
                    throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 1 and 125.");
                }
                Length = length;
            }

            public override byte[] GetBytes()
            {
                return new byte[]
                {
                    (byte)FunctionCode,
                    Address.HighByte(),
                    Address.LowByte(),
                    Length.HighByte(),
                    Length.LowByte()
                };
            }
        }


        public static ReadHoldingRegistersValidResponse ValidResponse(IEnumerable<Int16> data)
        {
            return new ReadHoldingRegistersValidResponse(data);
        }

        public static ReadHoldingRegistersValidResponse ValidResponse(PDU pdu)
        {
            if (pdu.FunctionCode != ModbusFunctionCode.ReadHoldingRegisters)
            {
                throw new MBInvalidFunctionCodeException(pdu.FunctionCode, "PDU must be a ReadHoldingRegisters response.");
            }
            if (pdu.Data.Count() < 3)
            {
                throw new ArgumentException($"PDU data must contain at least 3 bytes for a ReadHoldingRegisters response. PDU length: {pdu.Data.Count()}");
            }
            short byteCount = pdu.Data[0];
            if (byteCount < 2 || byteCount > 250 || byteCount % 2 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(byteCount), "Byte count must be between 2 and 250 and an even number.");
            }
            List<short> values = new List<short>();
            for (int i = 0; i < byteCount / 2; i++)
            {
                short value = Int16Extensions.FromBytes(pdu.Data[1 + (i * 2)], pdu.Data[2 + (i * 2)]);
                values.Add(value);
            }
            return new ReadHoldingRegistersValidResponse(values);
        }
        
        public class ReadHoldingRegistersValidResponse
            : PDU
        {
            public IEnumerable<Int16> Values{ get; private set; }
            internal ReadHoldingRegistersValidResponse(IEnumerable<Int16> values)
                : base(ModbusFunctionCode.ReadHoldingRegisters)
            {
                if (values.Count() < 1 || values.Count() > 125)
                {
                    throw new ArgumentOutOfRangeException(nameof(values), "Data must contain between 1 and 125 registers.");
                }
                Values = values;

                var byteList = new List<byte>();
                foreach (var value in values)
                {
                    byteList.Add(value.HighByte());
                    byteList.Add(value.LowByte());
                }

                Data = new byte[] 
                {
                    (byte)(values.Count() * 2),
                }.Concat(byteList).ToArray();

            }
        }


        public static ReadHoldingRegistersErrorResponse ErrorResponse(ExceptionCode exceptionCode)
        {
            return new ReadHoldingRegistersErrorResponse(exceptionCode);
        }

        public class ReadHoldingRegistersErrorResponse
            : PDU
        {
            public ExceptionCode ExceptionCode { get; private set; }
            public ReadHoldingRegistersErrorResponse(ExceptionCode exceptionCode)
                : base(ModbusFunctionCode.ReadHoldingRegisters)
            {
                if (exceptionCode != (ExceptionCode.IllegalFunction |
                                      ExceptionCode.IllegalDataAddress |
                                      ExceptionCode.IllegalDataValue |
                                      ExceptionCode.SlaveDeviceFailure))
                {
                    throw new ArgumentOutOfRangeException(nameof(exceptionCode), "Invalid exception code.");
                }
                ExceptionCode = exceptionCode;
            }


            public override byte[] GetBytes()
            {
                return new byte[]
                {
                    (byte)FunctionCode,
                    (byte)ExceptionCode
                };
            }
        }
    }
}
