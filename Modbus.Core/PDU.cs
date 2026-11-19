using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Modbus.Core
{
    //Maximum size of the PDU, regardless of message, is 253 bytes.
    //This restriction is based on the maximum size of a message in RS485, which is 256 bytes. 
    public class PDU
    {
        public ModbusFunctionCode FunctionCode { get; }

        public byte[] Data { get; protected set; } = new byte[0];

        protected PDU(ModbusFunctionCode functionCode)
        {
            FunctionCode = functionCode;
        }

        public PDU(IEnumerable<byte> receivedBytes)
        {
            if (receivedBytes.Count() < 1)
            {
                throw new ArgumentException("Received bytes must contain at least one byte for the function code.");
            }
            if (receivedBytes.Count() > 253)
            {
                throw new ArgumentException("Invalid PDU format. Size greater than 253 bytes.");
            }
            FunctionCode = (ModbusFunctionCode)receivedBytes.First();
            Data = new byte[receivedBytes.Count() - 1];
            Array.Copy(receivedBytes.ToArray(), 1, Data, 0, Data.Length);
        }

        public virtual byte[] GetBytes()
        {
            Console.WriteLine("Inside base PDU GetBytes");
            return new byte[]
            {
                (byte)FunctionCode
            }.Concat(Data).ToArray();
        }

        public bool IsExceptionResponse()
        {
            return (byte)FunctionCode >= 0x80;
        }

        public bool IsValidFunctionCode()
        {
            return Enum.IsDefined<ModbusFunctionCode>(FunctionCode);
        }

        public static PDU GetInvalidFunctionCodePDU(ModbusFunctionCode requestedFunction)
        {
            var errPDU = new PDU(ModbusFunctionCode.ErrorMask | requestedFunction);
            errPDU.Data = new byte[] { 0x01 };

            return errPDU;
        }
    }
}
