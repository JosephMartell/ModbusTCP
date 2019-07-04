using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ModbusTCP
{
    class ProtocolDataUnit
    {
        public ModbusFunctionCodes FunctionCode { get; }
        public IEnumerable<byte> Data { get; }

        public ProtocolDataUnit(ModbusFunctionCodes functionCode, IEnumerable<byte> data)
        {
            FunctionCode = functionCode;
            Data = data;
        }

        public ProtocolDataUnit(IEnumerable<byte> message)
        {
            FunctionCode = (ModbusFunctionCodes)message.Skip(7).First();
            Data = message.Skip(8);
        }

        public ProtocolDataUnit ValidResponse(IEnumerable<byte> data)
        {
            return new ProtocolDataUnit(FunctionCode, data);
        }

        public ProtocolDataUnit ErrorResponse(IEnumerable<byte> data)
        {
            return new ProtocolDataUnit(FunctionCode | ModbusFunctionCodes.ErrorMask, data);
        }

        public IEnumerable<byte> GetBytes()
        {
            return new byte[]
            {
                (byte)FunctionCode
            }.Concat(Data);
        }
    }
}
