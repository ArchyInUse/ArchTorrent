using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.PeerProtocol
{
    public enum PeerMessageId : byte
    {
        /// <summary>
        /// KeepAlive does not have a payload or ID
        /// </summary>
        KeepAlive = 255,
        Choke = 0,
        Unchoke = 1,
        Interested = 2,
        Uninterested = 3,
        Have = 4,
        Bitfield = 5,
        Request = 6,
        Piece = 7,
        Cancel = 8,
        Port = 9,
    }
}
