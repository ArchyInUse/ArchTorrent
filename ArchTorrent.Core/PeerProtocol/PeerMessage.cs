using ArchTorrent.Core.Trackers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.PeerProtocol
{
    public class PeerMessage
    {
        public byte[] Data { get; set; }
        public byte[] Payload { get; set; }

        /// <summary>
        /// Payload message length (first 4 bytes of Data)
        /// </summary>
        public int Length { get; set; }
        public byte MessageId { get; set; }
        public PeerMessageId Type { get; set; }
        public static PeerMessage ParseMessage(byte[] data)
        {
            // keep alive message
            if(data.Length == 4 && data.All(x => x == 0))
            {
                return new PeerMessage(Array.Empty<byte>());
            }
            Debug.Assert(data.Length >= 4);

            // returns choke unchoke interested and uninterested
            switch (data[4]) 
            { 
                case (byte)PeerMessageId.Choke:
                case (byte)PeerMessageId.Unchoke:
                case (byte)PeerMessageId.Interested:
                case (byte)PeerMessageId.Uninterested:
                    return new PeerMessage(data);
                default:
                    break;
            }
            return null;
            
        }

        public PeerMessage(byte[] data)
        {
            // keep-alive message
            if(data.Length == 0 || data[0..4].DecodeInt32() == 0)
            {
                Data = Array.Empty<byte>();
                Length = 0;
                MessageId = (byte)PeerMessageId.KeepAlive;
                Type = PeerMessageId.KeepAlive;
                Payload = Array.Empty<byte>();
                return;
            }

            Payload = data[5..];
            Length = data[0..4].DecodeInt32();
            MessageId = data[4];

            // if not keep-alive, assert data.Length >= 5 (minimum for 4 byte int + 1 raw id byte)
            Debug.Assert(data.Length >= 5);
        }
    }
}
