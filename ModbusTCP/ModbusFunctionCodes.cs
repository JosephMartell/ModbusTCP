using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusTCP
{
    enum ModbusFunctionCodes
        : byte
    {
        ReadCoil = 0x01,
        ReadInput = 0x02,
        ReadHoldingRegisters = 0x03,
        ReadInputRegisters = 0x04,
        ForceSingleCoil = 0x05,
        PresetSingleRegister = 0x06,
        ReadExceptionStatus = 0x07,
        Diagnostics = 0x08,
        Program484 = 0x09,
        Poll484 = 0x0a,
        FetchCommEventCounter = 0x0b,
        FetchCommEventLog = 0x0c,
        ProgramController = 0x0d,
        PollController = 0x0e,
        ForceMultipleCoils = 0x0f,
        PresetMultipleRegisters = 0x10,
        ReportSlaveID = 0x11,
        Program884 = 0x12,
        ResetCommLink = 0x13,
        ReadGeneralReference = 0x14,
        WriteGeneralReference = 0x15,
        MaskWrite4xRegister = 0x16,
        ReadWrite4xRegister = 0x17,
        ReadFIFOQueue = 0x18,
        ErrorMask = 0x80
    }
}
