﻿using BencodeNET.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.Trackers.UDPTrackerProtocol
{
    public struct AnnounceRequest
    {
        public readonly Int64 connection_id;

        // 1 = announce
        public readonly Int32 action = 1;

        public readonly Int32 transaction_id;

        // byte[20] (string)
        public readonly byte[] info_hash;

        // byte[20] (string)
        public readonly byte[] peer_id = new byte[20];

        public readonly Int64 downloaded;
        public readonly Int64 left;
        public readonly Int64 uploaded;

        /// <summary>
        /// 0: none; 1: completed; 2: started; 3: stopped
        /// </summary>
        public readonly Int32 _event; // _ to avoid name collision with keyword `event`

        public readonly Int32 ip_address; // 0 = default
        public readonly Int32 key; // random
        public readonly Int32 num_want; // -1 default

        public readonly Int16 port;

        public const int BYTE_COUNT = 98;

        public AnnounceRequest(Int64 connectionId, Torrents.Torrent torrent)
        {
            connection_id = connectionId;
            info_hash = TorrentUtil.CalculateInfoHashBytes(torrent.Info.OriginalDictionary);

            var uri = new Uri(torrent.AnnounceURL);
            port = (Int16)uri.Port;

            Random r = new Random();

            transaction_id = r.Next(Int32.MinValue, Int32.MaxValue);

            r.NextBytes(peer_id);
            peer_id = TrackerMessageHelpers.GenerateID();

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

        public byte[] Serialize()
        {
            // 98 byte message
            byte[] ret = new byte[BYTE_COUNT];
            Array.Copy(connection_id.EncodeInteger(), ret, 8);
            Array.Copy(action.EncodeInteger(), 0, ret, 8, 4);
            Array.Copy(transaction_id.EncodeInteger(), 0, ret, 12, 4);
            Array.Copy(info_hash, 0, ret, 16, 20);
            Array.Copy(peer_id, 0, ret, 36, 20);
            Array.Copy(downloaded.EncodeInteger(), 0, ret, 56, 8);
            Array.Copy(left.EncodeInteger(), 0, ret, 64, 8);
            Array.Copy(uploaded.EncodeInteger(), 0, ret, 72, 8);
            Array.Copy(_event.EncodeInteger(), 0, ret, 80, 4);
            Array.Copy(ip_address.EncodeInteger(), 0, ret, 84, 4);
            Array.Copy(key.EncodeInteger(), 0, ret, 88, 4);
            Array.Copy(num_want.EncodeInteger(), 0, ret, 92, 4);
            Array.Copy(port.EncodeInteger(), 0, ret, 96, 2);
            return ret;
        }
    }
}
