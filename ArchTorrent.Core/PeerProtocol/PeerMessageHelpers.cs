using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchTorrent.Core.Trackers;

namespace ArchTorrent.Core.PeerProtocol
{
    public static class PeerMessageHelpers
    {
        /// <summary>
        /// adds metadata to a peer protocol message (big endian)
        /// </summary>
        /// <param name="payload">payload of the message</param>
        /// <param name="id">message id</param>
        /// <returns></returns>
        public static byte[] MakeMessage(byte[] payload, PeerMessageId id)
        {
            if(id == PeerMessageId.KeepAlive)
            {
                // keep alive is only the length (which is 0)
                return new byte[4] { 0, 0, 0, 0 };
            }
            int size = -1;

            // 5 bytes for length (4 bytes) & id (1 raw byte)
            List<byte> ret = new ();

            switch(id)
            {
                case PeerMessageId.Choke:
                case PeerMessageId.Unchoke:
                case PeerMessageId.Interested:
                case PeerMessageId.Uninterested:
                    // choke: <len=0001><id=0>
                    // unchoke: <len=0001><id=1>
                    // interested: <len=0001><id=2>
                    // not interested: <len=0001><id=3>

                    // this conversion using EncodeInteger is because EncodeInteger ensures big endian
                    size = 1;
                    ret.AddRange(size.EncodeInteger());
                    ret.Add((byte)id);
                    return ret.ToArray();

                case PeerMessageId.Have:
                    // have: <len=0005><id=4><piece index>
                    // piece index has to be an integer, therefor assert that it is a 4 byte integer
                    // Debug.Assert(payload.Length == 4);
                    size = 5;
                    break;

                case PeerMessageId.Bitfield:
                    // bitfield: <len=0001+X><id=5><bitfield>
                    // x = size of the bitfield (+id byte)
                    size = 1 + payload.Length;
                    break;

                case PeerMessageId.Request:
                case PeerMessageId.Cancel:
                    // request: <len=0013><id=6><index><begin><length>
                    // cancel:  <len=0013><id=8><index><begin><length>
                    size = 13;
                    break;

                case PeerMessageId.Piece:
                    size = 9 + payload.Length;
                    break;

                case PeerMessageId.Port:
                    size = 3;
                    break;

                default:
                    size = -1;
                    break;
            }

            Debug.Assert(size > 0);

            ret.AddRange(size.EncodeInteger());
            ret.Add((byte)id);
            ret.AddRange(payload);

            return ret.ToArray();
        }
    }
}
