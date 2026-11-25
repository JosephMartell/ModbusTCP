using System;
using System.Collections.Generic;
using System.Text;

namespace Modbus.Core
{
    /// <summary>
    /// When inherited, provides minimal Modbus server functionality.
    /// This implementatoin does not handle any transport protocol.
    /// </summary>
    public abstract class ServerBase
    {
        public IMemoryModel MemoryModel { get; }

        protected ServerBase(IMemoryModel memoryModel)
        {
            MemoryModel = memoryModel;
        }

        /// <summary>
        /// Verifies received Protocol Data Units (PDU) and routes them
        /// to the appropriate methods for further processing.
        /// </summary>
        /// <param name="requestPDU">The request PDU from the client.</param>
        /// <returns>A PDU that may be a valid response or exception response.</returns>
        public virtual PDU ProcessRequest(PDU requestPDU)
        {
            if (!requestPDU.IsValidFunctionCode())
            {
                return PDU.GetInvalidFunctionCodePDU(requestPDU.FunctionCode);
            }


            //Each message handling function should perform error checks and produce the appropriate PDU response
            //based on the flow charts included in the Modbus specification.
            switch (requestPDU.FunctionCode)
            {
                case ModbusFunctionCode.ReadCoils:
                    return ReadCoils(ReadCoilsPDU.Request(requestPDU));
                case ModbusFunctionCode.ReadDiscreteInputs:
                    break;
                case ModbusFunctionCode.ReadHoldingRegisters:
                    break;
                case ModbusFunctionCode.ReadInputRegisters:
                    break;
                case ModbusFunctionCode.ForceSingleCoil:
                    break;
                case ModbusFunctionCode.PresetSingleRegister:
                    break;
                case ModbusFunctionCode.ReadExceptionStatus:
                    break;
                case ModbusFunctionCode.Diagnostics:
                    break;
                case ModbusFunctionCode.Program484:
                    break;
                case ModbusFunctionCode.Poll484:
                    break;
                case ModbusFunctionCode.FetchCommEventCounter:
                    break;
                case ModbusFunctionCode.FetchCommEventLog:
                    break;
                case ModbusFunctionCode.ProgramController:
                    break;
                case ModbusFunctionCode.PollController:
                    break;
                case ModbusFunctionCode.ForceMultipleCoils:
                    break;
                case ModbusFunctionCode.WriteMultipleRegisters:
                    break;
                case ModbusFunctionCode.ReportSlaveID:
                    break;
                case ModbusFunctionCode.Program884:
                    break;
                case ModbusFunctionCode.ResetCommLink:
                    break;
                case ModbusFunctionCode.ReadGeneralReference:
                    break;
                case ModbusFunctionCode.WriteGeneralReference:
                    break;
                case ModbusFunctionCode.MaskWrite4xRegister:
                    break;
                case ModbusFunctionCode.ReadWrite4xRegister:
                    break;
                case ModbusFunctionCode.ReadFIFOQueue:
                    break;
                case ModbusFunctionCode.ErrorMask:
                    break;
                default:
                    return PDU.GetInvalidFunctionCodePDU(requestPDU.FunctionCode);
            }
            return PDU.GetInvalidFunctionCodePDU(requestPDU.FunctionCode);

        }


        //There should be a 1:1 correlation between the methods here and the Modbus function codes supported.

        public virtual PDU ReadCoils(ReadCoilsPDU.ReadCoilsRequest request)
        {
            if (request.Quantity < 1 || request.Quantity > 2000)
            {
                return ReadCoilsPDU.ExceptionResponse(ExceptionCode.IllegalDataValue);
            }
            if (request.StartAddress < 0 || request.StartAddress + request.Quantity > 65536)
            {
                return ReadCoilsPDU.ExceptionResponse(ExceptionCode.IllegalDataAddress);
            }

            try
            {
                return ReadCoilsPDU.ValidResponse(MemoryModel.Read0x(request.StartAddress, request.Quantity));
            }
            catch
            {
                return ReadCoilsPDU.ExceptionResponse(ExceptionCode.ServerDeviceBusy);
            }
        }

        //public virtual PDU ReadDiscreteInputs(ReadDiscreteInputsPDU.ReadDiscreteInputsRequest pdu)
        //{
        //}

    }
}
