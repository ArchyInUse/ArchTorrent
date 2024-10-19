using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using ArchTorrent.Core.Trackers;

namespace ArchTorrent.Core.PeerProtocol
{
    public static class PeerMessageHelpers
    {
        public static byte AsByte(this PeerMessageId messageId)
        {
            return (byte)messageId;
        }

        public static byte[] AsNetworkOrder(this byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] b = (byte[])bytes.Clone();
                Array.Reverse(b);
                return b;
            }
            return bytes;
        }

        public static byte[] AsNetworkOrder(this int integer)
        {
            byte[] bytes = BitConverter.GetBytes(integer);
            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }
    }
}
