using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Torrents;

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

        public const string ATVERSION = "-AT0001-";

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

        public struct AnnounceRequest
        {
            public Int64 connection_id;

            // 1 = announce
            public Int32 action = 1;

            public Int32 transaction_id;

            // byte[20] (string)
            public byte[] info_hash;

            // byte[20] (string)
            public byte[] peer_id = new byte[20];

            public Int64 downloaded;
            public Int64 left;
            public Int64 uploaded;

            /// <summary>
            /// 0: none; 1: completed; 2: started; 3: stopped
            /// </summary>
            public Int32 _event; // _ to avoid name collision with keyword `event`

            public Int32 ip_address; // 0 = default
            public Int32 key; // random
            public Int32 num_want; // -1 default

            public Int16 port;

            public AnnounceRequest(Int64 connectionId, Torrents.Torrent torrent)
            {
                connection_id = connectionId;
                info_hash = TorrentUtil.CalculateInfoHashBytes(torrent.Info.OriginalDictionary);

                var uri = new Uri(torrent.AnnounceURL);
                port = (Int16)uri.Port;

                Random r = new Random();

                transaction_id = r.Next(Int32.MinValue, Int32.MaxValue);

                r.NextBytes(peer_id);
                var ATVerBytes = Encoding.ASCII.GetBytes(ATVERSION);
                
                // copy -AT0001- to the start of the array.
                Array.Copy(ATVerBytes, peer_id, ATVerBytes.Length);

                downloaded = 0;
                uploaded = 0;
                _event = 0;
                ip_address = 0;
                key = r.Next(Int32.MinValue, Int32.MaxValue);
                num_want = -1;

                var info = torrent.Info;
                left = 0;
                
                if(info.SingleFile)
                {
                    left += info.Files[0].Length;
                }
                else
                {
                    foreach(var file in info.Files)
                    {
                        left += file.Length;
                    }
                }
                
            }
        }

        public struct AnnounceResponse
        {
            // Offset      Size            Name            Value
            // 0           32-bit integer  action          1 // announce
            // 4           32-bit integer  transaction_id
            // 8           32-bit integer  interval
            // 12          32-bit integer  leechers
            // 16          32-bit integer  seeders
            // 20 + 6 * n  32-bit integer  IP address
            // 24 + 6 * n  16-bit integer  TCP port
            // 20 + 6 * N
            // Note: for IP & TCP ports, we'll group them up as they are related and read them together in IpPortPair
            Int32 action;
            Int32 transaction_id;
            Int32 interval;
            Int32 leechers;
            Int32 seeders;

            // these directly correspond to each other
            List<IpPortPair> endpoints;

            public AnnounceResponse(byte[] source)
            {
                action = BitConverter.ToInt32(source.ReadBytes(0, 4));
                transaction_id = BitConverter.ToInt32(source.ReadBytes(4, 4));
                interval = BitConverter.ToInt32(source.ReadBytes(8, 4));
                leechers = BitConverter.ToInt32(source.ReadBytes(12, 4));
                seeders = BitConverter.ToInt32(source.ReadBytes(16, 4));
                endpoints = new List<IpPortPair>();

                // begin at byte 20 to end, so the amount of ip pairs
                int ipAmount = (source.Length - 20) / 6;
                for(int i = 0; i < ipAmount; i++)
                {
                    byte[] bytes = source.ReadBytes(20 + (6 * i), 6);

                    var ip = bytes.ReadBytes(0, 4);
                    var port = bytes.ReadBytes(4, 2);
                    IpPortPair pair = new (BitConverter.ToInt32(ip), BitConverter.ToInt16(port));
                    endpoints.Add(pair);
                }
            }
        }

        public struct IpPortPair
        {
            Int32 ip;
            Int16 port;
            public IpPortPair(Int32 i, Int16 p)
            {
                ip = i;
                port = p;
            }
        }
    }
}
