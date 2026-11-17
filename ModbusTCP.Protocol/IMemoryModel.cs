using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusTCP
{
    /// <summary>
    /// Provides an interface to model underlying memory accessed by an instance of MBTCPServer.
    /// </summary>
    //TODO: Move this to the ModbusTCP.Server namespace. It is not a protocol concept.
    public interface IMemoryModel
    {
        IEnumerable<Int16> Read4x(int address, int length);
        void Write4x(int address, IEnumerable<Int16> values);
    }
}
