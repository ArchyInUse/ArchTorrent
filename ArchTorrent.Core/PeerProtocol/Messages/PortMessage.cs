using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.PeerProtocol.Messages
{
    internal class PortMessage : PeerMessage
    {
        public PortMessage(byte[] data, int length, byte messageId) : base(data)
        {
            throw new NotImplementedException();
        }
    }
}
