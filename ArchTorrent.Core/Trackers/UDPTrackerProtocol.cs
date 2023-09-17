using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.Trackers
{
    public static class UDPTrackerProtocol
    {
        /// <summary>
        /// reads the amount of data from the byte array and returns it
        /// </summary>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static byte[] ReadBytes(this byte[] source, int index, int amount)
        {
            byte[] res = new byte[amount];
            for(int i = index; i < (index + amount); i++)
            {
                res[i - index] = source[i];
            }
            return res;
        }

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

                action = new byte[]{ 0, 0, 0, 0 };
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

        public struct ConnectResponse
        {
            public Int32 action;
            public Int32 transaction_id;
            public Int64 connection_id;

            public static ConnectResponse Parse(byte[] resp)
            {
                ConnectResponse response = new ConnectResponse();
                byte[] action = resp.ReadBytes(0, 4);
                byte[] t_id = resp.ReadBytes(4, 4);
                byte[] c_id = resp.ReadBytes(8, 8);
                response.action = BitConverter.ToInt32(action);
                response.transaction_id = BitConverter.ToInt32(t_id);
                response.connection_id = BitConverter.ToInt64(c_id);

                return response;
            }
        }
    }
}
