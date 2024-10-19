using ArchTorrent.Core.Trackers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.PeerProtocol.Messages
{
    public class OneParamPeerMessage : PeerMessage
    {
        public byte[] Payload { get; set; }

        public OneParamPeerMessage(PeerMessageId id, byte[] payload) 
        {
            switch (id)
            {
                case PeerMessageId.Have:
                case PeerMessageId.Bitfield:
                case PeerMessageId.Port:
                    break;
                default:
                    throw new ArgumentException($"id {id} given to OneParameterMessage, wrong message type.");
            }
            MessageId = id;
            Payload = payload;
            Length = 1 + Payload.Length;
        }
        public override byte[] Encode()
        {
            List<byte> data = new List<byte>();
            data.AddRange(Length.AsNetworkOrder());
            data.Add((byte)MessageId);

            // although this is a shallow copy, the original doesn't get reversed because the type of the array is a value type.
            byte[] bigEndianPayload = (byte[])Payload.Clone();
            Array.Reverse(bigEndianPayload);

            data.AddRange(bigEndianPayload);
            return data.ToArray();
        }

        /// <summary>
        /// Parses an ordinary (big endian parts) peer message
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static OneParamPeerMessage Parse(byte[] data)
        {
            PeerMessageId id = (PeerMessageId)data[4];
            byte[] payload = data[5..];
            return new OneParamPeerMessage(id, payload);
        }
    }
}
