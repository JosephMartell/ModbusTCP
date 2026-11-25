namespace Modbus.Core
{
    public static class ReadCoilsPDU
    {

        public static ReadCoilsRequest Request(UInt16 startAddress, UInt16 quantity)
        {
            return new ReadCoilsRequest(startAddress, quantity);
        }

        public static ReadCoilsRequest Request(PDU pdu)
        {
            if (pdu.FunctionCode != ModbusFunctionCode.ReadCoils)
            {
                throw new ArgumentException($"Invalid function code for ReadCoils request: {pdu.FunctionCode}");
            }
            var bytes = pdu.GetBytes();
            if (bytes.Length < 4)
            {
                throw new ArgumentException("Invalid PDU length for ReadCoils request.");
            }
            UInt16 startAddress = BitConverter.ToUInt16(bytes, 0);
            UInt16 quantity = BitConverter.ToUInt16(bytes, 2);
            return new ReadCoilsRequest(startAddress, quantity);
        }

        public class ReadCoilsRequest
            : PDU
        {
            public UInt16 StartAddress { get; set; }
            public UInt16 Quantity { get; set; }
            internal ReadCoilsRequest(UInt16 startAddress, UInt16 quantity)
                : base(ModbusFunctionCode.ReadCoils)
            {
                StartAddress = startAddress;
                Quantity = quantity;

                Data = new byte[] {
                    startAddress.HighByte(),
                    startAddress.LowByte(),
                    quantity.HighByte(),
                    quantity.LowByte()
                };
            }
        }


        public static ReadCoilsValidResponse ValidResponse(IEnumerable<bool> coilStatus)
        {
            return new ReadCoilsValidResponse(coilStatus);
        }

        public static ReadCoilsValidResponse ValidResponse(PDU pdu)
        {
            if (pdu.FunctionCode != ModbusFunctionCode.ReadCoils)
            {
                throw new ArgumentException($"Invalid function code for ReadCoils response: {pdu.FunctionCode}");
            }
            var bytes = pdu.GetBytes();
            if (bytes.Length < 3)
            {
                throw new ArgumentException($"Invalid PDU length for ReadCoils resposne.");
            }

            ushort byteCount = bytes.Skip(1).First();
            if (bytes.Count() - 2 != byteCount)
            {
                throw new ArgumentException($"Byte count does not match length of PDU");
            }
            List<bool> coilStatus = new List<bool>();

            foreach (var b in bytes.Skip(2))
            {
                for (int i = 0; i < 8; i++)
                {
                    coilStatus.Add(((1 << i) & b) > 0);
                }
            }

            return new ReadCoilsValidResponse(coilStatus);
        }

        public class ReadCoilsValidResponse
            : PDU
        {
            public IEnumerable<bool> CoilStatus { get; set; }
            public ReadCoilsValidResponse(IEnumerable<bool> coilStatus)
                : base(ModbusFunctionCode.ReadCoils)
            {
                CoilStatus = coilStatus;
                Math.Ceiling(coilStatus.Count() / 8.0);

                List<byte> bytes = new List<byte>();

                byte workingByte = 0;
                for (int i = 0; i < coilStatus.Count(); i++)
                {
                    if (i % 8 == 0 && i != 0)
                    {
                        bytes.Add(workingByte);
                        workingByte = 0;
                    }
                    if (coilStatus.ElementAt(i))
                    {
                        workingByte = (byte)((byte)(workingByte << 1) | 1);
                    }
                    else
                    {
                        workingByte = (byte)(workingByte << 1);
                    }
                }
                bytes.Add(workingByte);
                Data = new byte[] {
                    (byte)bytes.Count()
                }.Concat(bytes).ToArray();
            }
        }


        public static ReadCoilsExceptionResponse ExceptionResponse(ExceptionCode exceptionCode)
        {
            return new ReadCoilsExceptionResponse(exceptionCode);
        }

        public class ReadCoilsExceptionResponse
            : PDU
        {
            public ExceptionCode ExceptionCode { get; set; }
            public ReadCoilsExceptionResponse(ExceptionCode exceptionCode)
                : base(ModbusFunctionCode.ReadCoils)
            {
                ExceptionCode = exceptionCode;
            }

            public override byte[] GetBytes()
            {
                throw new NotImplementedException();
            }
        }
    }
}
