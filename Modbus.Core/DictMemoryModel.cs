using System;
using System.Collections.Generic;
using System.Linq;
using Modbus.Core;

namespace Modbus.Core
{
    //TODO: Because of recent refactoring, tests should be added to verify the validity of individual message responses
    public class DictMemoryModel
        : IMemoryModel
    {
        Dictionary<UInt16, bool> _0xMemory = new Dictionary<UInt16, bool>();
        Dictionary<int, Int16> _4xMemeory = new Dictionary<int, short>();


        public IEnumerable<bool> Read0x(UInt16 address, UInt16 length)
        {
            bool[] values = new bool[length];
            for (int i = 0; i < values.Length; i++)
            {
                if (_0xMemory.ContainsKey((UInt16)(i + address)))
                {
                    values[i] = _0xMemory[(UInt16)(i + address)];
                }
                else
                {
                    values[i] = false;
                }
            }
            return values;
        }

        public void Write0x(UInt16 address, IEnumerable<bool> values)
        {
            UInt16 offset = 0;
            values.ToList().ForEach(v =>
            {
                _0xMemory[(UInt16)(address + offset)] = v;
                offset++;
            });
        }

        public IEnumerable<short> Read4x(UInt16 address, UInt16 length)
        {
            short[] values = new short[length];
            for (int i = 0; i < values.Length; i++)
            {
                if (_4xMemeory.ContainsKey(i + address))
                {
                    values[i] = _4xMemeory[i + address];
                }
                else
                {
                    values[i] = 0;
                }
            }
            return values;
        }

        public void Write4x(UInt16 address, IEnumerable<short> values)
        {
            int offset = 0;
            values.ToList().ForEach(v => {
                _4xMemeory[address + offset] = v;
                offset++;
            });
        }
    }
}
