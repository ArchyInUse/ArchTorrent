using ArchTorrent.Core.Trackers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.PeerProtocol.Messages
{
    public class CancelMessage : PeerMessage
    {
        public readonly int Index;
        public readonly int Begin;
        public readonly int RequestLength;
        public CancelMessage(byte[] data) : base(data)
        {
            Index = Payload[0..4].DecodeInt32();
            Begin = Payload[4..8].DecodeInt32();
            RequestLength = Payload[8..12].DecodeInt32();

        }
    }
}
