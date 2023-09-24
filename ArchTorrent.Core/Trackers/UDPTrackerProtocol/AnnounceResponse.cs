using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.Trackers.UDPTrackerProtocol
{
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
        public List<Peer> peers;

        public AnnounceResponse(byte[] source)
        {
            action = source.ReadBytes(0, 4).DecodeInt32();
            transaction_id = source.ReadBytes(4, 4).DecodeInt32();
            interval = source.ReadBytes(8, 4).DecodeInt32();
            leechers = source.ReadBytes(12, 4).DecodeInt32();
            seeders = source.ReadBytes(16, 4).DecodeInt32();
            peers = new List<Peer>();

            // begin at byte 20 to end, so the amount of ip pairs
            int ipAmount = (source.Length - 20) / 6;
            for (int i = 0; i < ipAmount; i++)
            {
                byte[] bytes = source.ReadBytes(20 + (6 * i), 6);

                var ip = bytes.ReadBytes(0, 4);
                var port = bytes.ReadBytes(4, 2);
                Peer pair = new(ip.DecodeInt32(), port.DecodeInt16());
                peers.Add(pair);
            }
        }
    }
}
