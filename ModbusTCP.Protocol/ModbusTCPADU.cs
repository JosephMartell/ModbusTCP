using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ModbusTCP.Protocol;
using Modbus.Core;

namespace ModbusTCP.Protocol
{
    public class ModbusTCPADU
    {
        public MBAP ApplicationHeader { get; }
        public PDU PDU { get; }

        protected ModbusTCPADU(MBAP appHeader, PDU pdu)
        {
            ApplicationHeader = appHeader;
            PDU = pdu;
        }
        
        public IEnumerable<byte> GetBytes()
        {
            return ApplicationHeader.GetBytes().Concat(PDU.GetBytes());
        }

        //The ADU cannot exceed 260 bytes.
        //The Minimum size is 9 bytes: 7 bytes for the header + 2 for an error response.
        public static ModbusTCPADU Deserialize(IEnumerable<byte> buffer)
        {
            if (buffer.Count() < 9 || buffer.Count() > 260)
            {
                throw new ArgumentOutOfRangeException($"Modbus TCP messages must be between 9 and 260 bytes, inclusive. Message length: {buffer.Count()}");
            }
            return new ModbusTCPADU(MBAP.ParseFromMessage(buffer.Take(7)), new PDU(buffer.Skip(7)));
        }

        public static ModbusTCPADU Wrap(Int16 transactionID, PDU pdu)
        {
            MBAP mbap = MBAP.Create(transactionID, (Int16)(pdu.GetBytes().Count() + 1), 0);
            return new ModbusTCPADU(mbap, pdu);
        }

        public static ModbusTCPADU RespondTo(ModbusTCPADU request, PDU responsePDU)
        {
            var mbap = MBAP.Create(request.ApplicationHeader.TransactionID, (short)(responsePDU.GetBytes().Count() + 1), request.ApplicationHeader.UnitID);
            return new ModbusTCPADU(mbap, responsePDU);
        }
    }
}
