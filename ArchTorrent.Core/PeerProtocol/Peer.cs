using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ArchTorrent.Core.Torrents;
using ArchTorrent.Core.Trackers;

namespace ArchTorrent.Core.PeerProtocol
{
    public class Peer
    {
        public IPAddress Ip { get; set; }
        public int Port { get; set; }
        public Socket Sock { get; set; }
        public string InfoHash { get; set; }

        // 32kb buffer
        private byte[] buffer = new byte[1024 * 32];

        public Peer(byte[] ip, Int16 port, string infoHash)
        {
            Ip = new IPAddress(ip);
            Port = port;
            Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            InfoHash = infoHash;
        }

        public override string ToString()
        {
            return $"Peer: {Ip}:{Port}";
        }

        public async Task<bool> InitDownloadAsync(CancellationToken cts)
        {
            // choke / unchoke -> peer does not want / does want to give a piece
            // interested / uninterested -> we want / do not want a piece
            // 1. handshake
            // 2. have / bitfield messages
            
            await Sock.ConnectAsync(new IPEndPoint(Ip, Port));
            if (await Sock.SendAsync(ConstructHandshake(), SocketFlags.None, cts) == 0)
            {
                Logger.Log($"Critical Error: could not send handshake message to {Ip}:{Port}!", source: "Peer Handshake");
                return false;
            }
            byte[] handshakeBuffer = new byte[1024];
            int handshakeRecieved = await Sock.ReceiveAsync(handshakeBuffer, SocketFlags.None, cts);

            Array.Resize(ref handshakeBuffer, handshakeRecieved);

            var asStr = Encoding.ASCII.GetString(handshakeBuffer);
            Logger.Log($"Handshake Response: {asStr}", source:"Peer Handshake");

            // TODO: message error handling, maybe an IError interface on all of them to signal an error with a boolean

            // see if recieved the entire 
            if(!(handshakeBuffer[0] + 49 == handshakeRecieved && Encoding.ASCII.GetString(handshakeBuffer.ReadBytes(1, handshakeBuffer[0])) == "BitTorrent protocol"))
            {
                Logger.Log($"Parsing went wrong!", source: "Peer Handshake");
                return false;
            }

            Logger.Log($"Handshake recieved correctly; starting download now", source: "Peer Handshake");
            Logger.Log($"Now able to init download");
            return true;
        }

        // handshake is very simple so no need for class
        private byte[] ConstructHandshake()
        {

            // handshake: <pstrlen><pstr><reserved><info_hash><peer_id>

            // pstrlen: string length of<pstr>, as a single raw byte
            // pstr: string identifier of the protocol
            // reserved: eight(8) reserved bytes. All current implementations use all zeroes.
            // info hash
            // peer_id: 20 - byte string used as a unique ID for the client.
            // 
            // 
            // In version 1.0 of the BitTorrent protocol, pstrlen = 19, and pstr = "BitTorrent protocol".

            List<byte> data = new List<byte>();

            // pstrlen (constant)
            data.Add(19);

            // pstr
            var pstr = Encoding.ASCII.GetBytes("BitTorrent protocol");
            foreach(byte b in pstr) { data.Add(b); }

            // reserved
            for(int i = 0; i < 8; i++) { data.Add(0); }

            var hash = Encoding.ASCII.GetBytes(InfoHash);
            for(int i = 0; i < hash.Length; i++) { data.Add(hash[i]); }

            var peer_id = TrackerMessageHelpers.GenerateID();
            foreach(byte b in peer_id) { data.Add(b); }

            return data.ToArray();
        }
    }
}
