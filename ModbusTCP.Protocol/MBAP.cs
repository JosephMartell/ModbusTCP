using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus.Core;

namespace ModbusTCP.Protocol
{
    /// <summary>
    /// MODBUS Application Protocol header is used to identify a TCP packet as
    /// a MODBUS message.
    /// </summary>
    public class MBAP
    {
        public Int16 TransactionID { get; private set; }
        public const Int16 ProtocolIdentifier  = 0;

        public Int16 Length { get; private set; }
        public Byte UnitID { get; } = 0;

        protected MBAP(Int16 length, byte unitID)
        {
            Length = length;
            UnitID = unitID;
        }

        protected MBAP(Int16 length, byte unitID, Int16 transactionID)
        {
            Length = length;
            UnitID = unitID;
            TransactionID = transactionID;
        }

        //TODO: Move this parsing logic to the static method. Limit MBAP to a single constructor.
        protected MBAP(IEnumerable<byte> message)
        {
            Length = Int16Extensions.FromBytes(message.Skip(4).First(), message.Skip(5).First());
            UnitID = message.Skip(6).First();
        }

        public byte[] GetBytes()
        {
            return new byte[] 
            { 
                TransactionID.HighByte(), 
                TransactionID.LowByte(), 
                ProtocolIdentifier.HighByte(), 
                ProtocolIdentifier.LowByte(), 
                Length.HighByte(), 
                Length.LowByte(), 
                UnitID };
        }

        public static MBAP Create(Int16 transactionID, Int16 length, byte unitID)
        {
            MBAP mbap = new MBAP(length, unitID, transactionID);
            return mbap;
        }

        //TODO: What validation should be added here?
        public static MBAP ParseFromMessage(IEnumerable<byte> message)
        {
            return new MBAP(message);
        }
    }
}
