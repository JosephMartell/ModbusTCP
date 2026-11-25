using System;
using System.Collections.Generic;
using System.Text;

namespace Modbus.Core
{
    /// <summary>
    /// Provides methods for accessing a memory model consistent with the Modbus protocol. This represents 
    /// a mechanism to handle PDU requests and repsonses independent of the transport layer.
    /// </summary>
    public interface IMemoryModel
    {
        IEnumerable<bool> Read0x(UInt16 address, UInt16 length);
        void Write0x(UInt16 address, IEnumerable<bool> values);

        IEnumerable<Int16> Read4x(UInt16 address, UInt16 length);
        void Write4x(UInt16 address, IEnumerable<Int16> values);
    }
}
