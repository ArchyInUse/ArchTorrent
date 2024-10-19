using ArchTorrent.Core.Trackers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.PeerProtocol.Messages
{
    public class ThreeParamPeerMessage : PeerMessage
    {
        public int Index { get; set; }
        public int Begin { get; set; }

        /// <summary>
        /// This parameter can be either Block or Length, where Block is a block of data of unspecified 
        /// </summary>
        public byte[] Param3 { get; set; }

        public int Param3AsInt { get => Param3.DecodeInt32(); }
        public ThreeParamPeerMessage(PeerMessageId id, int index, int begin, byte[] param3) 
        {
            switch (id)
            {
                case PeerMessageId.Piece:
                case PeerMessageId.Cancel:
                case PeerMessageId.Request:
                    break;
                default:
                    throw new ArgumentException($"id {id} given to ThreeParameterMessage, wrong message type.");
            }
            MessageId = id;
            Index = index;
            Begin = begin;
            Param3 = param3;
            // id (1 byte) + index (4 bytes) + begin (4 bytes) + param3 size => 9 + param3 size 
            Length = 9 + param3.Length;
        }

        public override byte[] Encode()
        {
            List<byte> data = new List<byte>();
            data.AddRange(Length.AsNetworkOrder());
            data.Add((byte)MessageId);
            // each section needs to be big endian *individually*
            data.AddRange(Index.AsNetworkOrder());
            data.AddRange(Begin.AsNetworkOrder());
            data.AddRange(Param3.AsNetworkOrder());
            return data.ToArray();
        }
        
        public static ThreeParamPeerMessage Parse(byte[] data)
        {
            // first 4 bytes reserved for length
            int length = data[0..4].DecodeInt32();

            // 5th byte is id
            PeerMessageId id = (PeerMessageId)data[4];
            
            // we'll take the payload as is for simplicity
            byte[] payload = data[5..];
            // first param is an integer (4 bytes)
            int index = payload[0..4].DecodeInt32();
            // second param is also an integer (4 bytes)
            int begin = payload[4..8].DecodeInt32();
            // third param can be either data or an integer, leaving it as is
            byte[] param3 = data[8..];

            return new ThreeParamPeerMessage(id, index, begin, param3);

        }
    }
}
