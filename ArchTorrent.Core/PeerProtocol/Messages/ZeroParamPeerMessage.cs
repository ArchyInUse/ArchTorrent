using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.PeerProtocol.Messages
{
    public class ZeroParamPeerMessage : PeerMessage
    {
        public const int Length = 1;
        public ZeroParamPeerMessage(PeerMessageId id) 
        {
            switch (id)
            {
                case PeerMessageId.Choke:
                case PeerMessageId.Unchoke:
                case PeerMessageId.Interested:
                case PeerMessageId.Uninterested:
                    break;
                default:
                    throw new ArgumentException($"id {id} given to OneParameterMessage");
            }
            MessageId = id;
        }

        public override byte[] Encode()
        {
            List<byte> data = new List<byte>(5);
            data.AddRange(Length.AsNetworkOrder()); 
            data.Add((byte)MessageId);
            return data.ToArray();
        }

        public static ZeroParamPeerMessage Parse(byte[] data)
        {
            return new ZeroParamPeerMessage((PeerMessageId)data[5]); // endianess doesnt matter on 1 byte
        }
    }
}
