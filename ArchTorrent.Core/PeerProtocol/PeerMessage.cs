using ArchTorrent.Core.Trackers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.PeerProtocol
{
    /// <summary>
    /// A peer message will always follow the format of:
    /// <Length><Id><Payload>
    /// 4 bytes -> 1 byte -> payload
    /// </summary>
    public abstract class PeerMessage
    {
        public abstract byte[] Encode();
        public PeerMessageId MessageId { get; set; }
        public int Length { get; set; }
    }
}
