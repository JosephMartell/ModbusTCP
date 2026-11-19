namespace Modbus.Core
{
    public static class ReadCoilsPDU
    {

        public static ReadCoilsRequest Request(short startAddress, short quantity)
        {
            return new ReadCoilsRequest(startAddress, quantity);
        }

        public class ReadCoilsRequest
            : PDU
        {
            public short StartAddress { get; set; }
            public short Quantity { get; set; }
            internal ReadCoilsRequest(short startAddress, short quantity)
                : base(ModbusFunctionCode.ReadCoil)
            {
                StartAddress = startAddress;
                Quantity = quantity;
            }

            public override byte[] GetBytes()
            {
                throw new NotImplementedException();
            }
        }


        public static ReadCoilsValidResponse ValidResponse(IEnumerable<bool> coilStatus)
        {
            return new ReadCoilsValidResponse(coilStatus);
        }

        public class ReadCoilsValidResponse
            : PDU
        {
            public IEnumerable<bool> CoilStatus { get; set; }
            public ReadCoilsValidResponse(IEnumerable<bool> coilStatus)
                : base(ModbusFunctionCode.ReadCoil)
            {
                CoilStatus = coilStatus;
                Math.Ceiling(coilStatus.Count() / 8.0);
            }

            public override byte[] GetBytes()
            {
                throw new NotImplementedException();
            }
        }


        public static ReadCoilsExceptionResponse ExceptionResponse(byte exceptionCode)
        {
            return new ReadCoilsExceptionResponse(exceptionCode);
        }

        public class ReadCoilsExceptionResponse
            : PDU
        {
            public byte ExceptionCode { get; set; }
            public ReadCoilsExceptionResponse(byte exceptionCode)
                : base(ModbusFunctionCode.ReadCoil)
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
