using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.Trackers.UDPTrackerProtocol
{
    public struct ConnectRequest
    {
        public Int64 protocol_id = 0x41727101980;

        // 0 = connect
        public Int32 action = 0;

        public Int32 transaction_id;

        public ConnectRequest()
        {
            // random 32-bit integer
            Random r = new Random();
            transaction_id = r.Next(Int32.MinValue, Int32.MaxValue);
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[16];
            byte[] protocolId, action, transactionId;

            protocolId = BitConverter.GetBytes(protocol_id);

            action = new byte[] { 0, 0, 0, 0 };
            transactionId = BitConverter.GetBytes(transaction_id);

            bytes = protocolId.Concat(action.Concat(transactionId)).ToArray();
            if (bytes.Length > 16) throw new Exception("Wrong length");

            return bytes;
        }

        /// <summary>
        /// checks if the transaction ids match
        /// </summary>
        /// <param name="cr"></param>
        /// <returns></returns>
        public bool CheckResponse(ConnectResponse cr) => cr.transaction_id == transaction_id;
    }
}
