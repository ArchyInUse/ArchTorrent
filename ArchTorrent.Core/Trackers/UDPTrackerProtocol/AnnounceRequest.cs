using BencodeNET.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.Trackers.UDPTrackerProtocol
{
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
            var ATVerBytes = Encoding.ASCII.GetBytes(TrackerUtils.ATVERSION);

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

            if (info.SingleFile)
            {
                left += info.Files[0].Length;
            }
            else
            {
                foreach (var file in info.Files)
                {
                    left += file.Length;
                }
            }

        }
    }
}
