using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ArchTorrent.Core.Trackers;

namespace ArchTorrent.Core.PeerProtocol
{
    public class Peer
    {
        public IPAddress Ip { get; set; }
        public int Port { get; set; }
        public TcpClient Client { get; set; }

        public Peer(byte[] ip, Int16 port)
        {
            Ip = new IPAddress(ip);
            Port = port;
            Client = new TcpClient();
        }

        public override string ToString()
        {
            return $"Peer: {Ip}:{Port}";
        }

        public async Task InitDownloadAsync()
        {
            // choke / unchoke -> peer does not want / does want to give a piece
            // interested / uninterested -> we want / do not want a piece
            // 1. handshake
            // 2. have / bitfield messages
            await Client.ConnectAsync(new IPEndPoint(Ip, Port));

        }

        // handshake is very simple so no need for class
        private byte[] ConstructHandshake()
        {

            // handshake: <pstrlen><pstr><reserved><info_hash><peer_id>

            // pstrlen: string length of<pstr>, as a single raw byte
            // pstr: string identifier of the protocol
            // reserved: eight(8) reserved bytes. All current implementations use all zeroes.
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

            var peer_id = TrackerMessageHelpers.GenerateID();
            foreach(byte b in peer_id) { data.Add(b); }

            return data.ToArray();
        }
    }
}
