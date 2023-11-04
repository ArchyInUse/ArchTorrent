using ArchTorrent.Core.Trackers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.PeerProtocol.Messages
{
    public class BitfieldMessage : PeerMessage
    {
        public readonly byte[] Bitfield;
        public BitfieldMessage(byte[] data) : base(data)
        {
            // read the rest of the array for the bitfield
            Bitfield = data[5..];
        }
    }
}
