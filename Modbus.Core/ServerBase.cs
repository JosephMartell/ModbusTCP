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
                    return ReadHoldingRegisters(ReadHoldingRegistersPDU.Request(requestPDU));
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
                    return WriteMultipleRegisters(WriteMultipleRegistersPDU.Request(requestPDU));
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

        //0x01 - Read Coils (0x01)
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


        //0x02 - Read Discrete Inputs
        //0x03 - Read Holding Registers
        public virtual PDU ReadHoldingRegisters(ReadHoldingRegistersPDU.ReadHoldingRegistersRequest request)
        {
            if (request.Length < 1 || request.Length > 125)
            {
                return ReadHoldingRegistersPDU.ExceptionResponse(ExceptionCode.IllegalDataValue);
            }
            if (request.Address < 0 || request.Address + request.Length > 65536)
            {
                return ReadHoldingRegistersPDU.ExceptionResponse(ExceptionCode.IllegalDataAddress);
            }

            try
            {
                return ReadHoldingRegistersPDU.ValidResponse(MemoryModel.Read4x(request.Address, request.Length));
            }
            catch
            {
                return ReadHoldingRegistersPDU.ExceptionResponse(ExceptionCode.ServerDeviceBusy);
            }

        }
        //0x04 - Read Input Registers
        //0x05 - Write Single Coil
        //0x06 - Write Single Register
        //0x07 - Read Exception Status (Serial only - do not implement)
        //0x08 - Diagnostics (serial only - do not implement)
        //0x0b - Get CommEvent Counter (serial only - do not implement)
        //0x0c - Get Comm Event Log (serial only - do not implement)
        //0x0f - Write Multiple Coils
        //0x10 - Write Multiple Registers
        public virtual PDU WriteMultipleRegisters(WriteMultipleRegistersPDU.WriteMultipleRegistersRequest request)
        {
            if (request.Values.Count() < 1 || request.Values.Count() > 123)
            {
                return WriteMultipleRegistersPDU.ExceptionResponse(ExceptionCode.IllegalDataValue);
            }
            if (request.StartAddress < 0 || request.StartAddress + request.Values.Count() > 65536)
            {
                return WriteMultipleRegistersPDU.ExceptionResponse(ExceptionCode.IllegalDataAddress);
            }

            try
            {
                MemoryModel.Write4x(request.StartAddress, request.Values);
                return WriteMultipleRegistersPDU.ValidResponse(request.StartAddress, (UInt16)request.Values.Count());
            }
            catch
            {
                return WriteMultipleRegistersPDU.ExceptionResponse(ExceptionCode.ServerDeviceBusy);
            }

        }

        //0x11 - Report Server ID (Serial only - do not implement)
        //0x14 - Read File Record
        //0x15 - Write File Record
        //0x16 - Mask Write Register
        //0x17 - Read/Write Multiple Registers
        //0x18 - Read FIFO Queue
        //0x2b - Encapslated Interface Transport (see below)
        //  0x0d CANOpen General Reference Request and Response
        //  0x0e Read Device Identification

    }
}
