using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ModbusTCP
{
    class ModbusTCPMessage
    {
        public MBAP ApplicationHeader { get; }
        public ProtocolDataUnit PDU { get; }

        protected ModbusTCPMessage(MBAP appHeader, ProtocolDataUnit pdu)
        {
            ApplicationHeader = appHeader;
            PDU = pdu;
        }

        protected ModbusTCPMessage(IEnumerable<byte> messageBuffer)
        {
            ApplicationHeader = MBAP.ParseFromMessage(messageBuffer);
            PDU = new ProtocolDataUnit(messageBuffer);
        }
        
        public IEnumerable<byte> GetBytes()
        {
            return ApplicationHeader.GetBytes().Concat(PDU.GetBytes());
        }

        public virtual ModbusTCPMessage Process(IMemoryModel memory) { return this; }

        public static ModbusTCPMessage ParseBytes(IEnumerable<byte> message)
        {
            var og = new ModbusTCPMessage(message);
            ModbusFunctionCodes func = og.PDU.FunctionCode;
            switch (func)
            {
                case ModbusFunctionCodes.ReadHoldingRegisters:
                    return new ReadRegistersMessage(message);
                case ModbusFunctionCodes.PresetMultipleRegisters:
                    return new WriteHoldingRegistersMessage(message);
                default:
                    return new ModbusTCPMessage(message);
            }
        }

        public static ModbusTCPMessage Create(MBAP appHeader, ProtocolDataUnit pdu)
        {
            switch (pdu.FunctionCode)
            {
                case ModbusFunctionCodes.ReadHoldingRegisters:
                    return new ReadRegistersMessage(appHeader, pdu);
                case ModbusFunctionCodes.PresetMultipleRegisters:
                    return new WriteHoldingRegistersMessage(appHeader, pdu);
                default:
                    return new ModbusTCPMessage(appHeader, pdu);
            }
        }
    }
}
