using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public const int BYTE_COUNT = 16;

        public ConnectRequest()
        {
            // random 32-bit integer
            Random r = new Random();
            transaction_id = r.Next(Int32.MinValue, Int32.MaxValue);
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[BYTE_COUNT];
            byte[] protocolId, action, transactionId;

            protocolId = protocol_id.EncodeInteger();
            transactionId = transaction_id.EncodeInteger();
            action = new byte[] { 0, 0, 0, 0 };
            
            bytes = protocolId.Concat(action.Concat(transactionId)).ToArray();
            string s = "";
            for(int i = 0; i < bytes.Length; i++)
            {
                s += bytes[i].ToString("X2");
            }
            Logger.Log($"Sending: {s}", source: "Serialize Connection Request");
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
