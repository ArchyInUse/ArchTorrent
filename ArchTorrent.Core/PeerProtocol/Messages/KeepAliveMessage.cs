using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.PeerProtocol.Messages
{
    public class KeepAliveMessage : PeerMessage
    {
        public override byte[] Encode() => Array.Empty<byte>();
    }
}
