using System;
using System.Collections.Generic;
using System.Text;

namespace Modbus.Core
{
    public enum ExceptionCode
        : byte
    {
        IllegalFunction = 0x01,
        IllegalDataAddress = 0x02,
        IllegalDataValue = 0x03,
        ServerDeviceFailure = 0x04,
        Acknowledge = 0x05,
        ServerDeviceBusy = 0x06,
        MemoryParityError = 0x08,
        GatewayPathUnavailable = 0x0a,
        GatewayTargetDeviceFailedToRespond = 0x0b
    }

    public class MBInvalidFunctionCodeException
        : Exception
    {
        public ModbusFunctionCode FunctionCode { get; private set; }
        public MBInvalidFunctionCodeException(ModbusFunctionCode functionCode, string message)
            : base(message)
        {
            FunctionCode = functionCode;
        }
    }

}
