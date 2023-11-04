using ArchTorrent.Core.Trackers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.PeerProtocol.Messages
{
    public class HaveMessage : PeerMessage
    {
        public readonly int Index;
        public HaveMessage(byte[] data) : base(data)
        {
            // if(Payload.Length != 4) { error }

            // message length = 5 -> 1 id byte + 4 bytes for Index
            Index = Payload.DecodeInt32();
        }
    }
}
