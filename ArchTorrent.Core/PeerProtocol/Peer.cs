using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ArchTorrent.Core.Torrents;
using ArchTorrent.Core.Trackers;

/// From my research the usual way that a peer piece exchange works is as follows:
/// 1. Handshake
/// 2. Send a bitfield 
/// 3. Receive a bitfield (parse)
/// 4. Send a request for a piece
/// 5. Receive a piece message
/// 

namespace ArchTorrent.Core.PeerProtocol
{
    public class Peer : IEquatable<Peer>
    {
        public IPAddress Ip { get; set; }
        public int Port { get; set; }
        public Socket Sock { get; set; }
        public string InfoHash { get; set; }
        private Tracker tracker;
        private TorrentBitField bitfield { get => tracker.Torrent.Bitfield; }
        public bool IsConnected { get => Sock.Connected; }

        public bool Am_Choking { get; private set; } = true;
        public bool Am_Interested { get; private set; } = false;
        /// <summary>
        /// when a peer chokes the client, it is a notification that no requests will be answered
        /// until the client is unchoked, The client should not attemt to send requests for blocks and it should consider all
        /// pending requests to be discarded by the remote peer.
        /// </summary>
        public bool Peer_Choking { get; private set; } = true;
        public bool Peer_Interested { get; private set; } = false;

        /// <summary>
        /// Optional, and will potentially not be in use.
        /// </summary>
        public string peerId { get; set; } = "";

        // 32kb buffer
        private byte[] buffer = new byte[1024 * 32];

        private byte[] peer_Bitfield;
        private bool bitfieldReceived = false;

        public Peer(byte[] ip, Int16 port, string infoHash, Tracker tracker, string id = "")
        {
            Ip = new IPAddress(ip);
            Port = port;
            Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            InfoHash = infoHash;
            peerId = id;
            this.tracker = tracker;
        }

        public Peer(byte[] ip, byte[] port, string infoHash, Tracker tracker, string id = "")
        {
            Ip = new IPAddress(ip);
            Port = TrackerMessageHelpers.DecodeInt16(port);
            Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            InfoHash = infoHash;
            peerId = id;
            this.tracker = tracker;
        }

        public override string ToString()
        {
            return $"Peer: {Ip}:{Port}";
        }

        public async Task<bool> HandshakePeer(CancellationToken cts)
        {
            // choke / unchoke -> peer does not want / does want to give a piece
            // interested / uninterested -> we want / do not want a piece
            // 1. handshake
            // 2. have / bitfield messages
            Logger.Log($"Begin connection request...", source: "Peer Handshake");
            try
            {
                await Sock.ConnectAsync(new IPEndPoint(Ip, Port));
            }
            catch (SocketException)
            {
                Logger.Log($"Peer {Ip}:{Port} is unresponsive, destroying peer.", source: "Peer Handshake");
                tracker.Peers.Remove(this);
                return false;
            }
            byte[] localHandshake = ConstructHandshake();
            if (await Sock.SendAsync(localHandshake, SocketFlags.None, cts) == 0)
            {
                Logger.Log($"Critical Error: could not send handshake message to {Ip}:{Port}!", source: "Peer Handshake");
                tracker.Peers.Remove(this);
                return false;
            }
            byte[] handshakeBuffer = new byte[1024];
            int handshakeRecieved = -1;
            try
            {
                handshakeRecieved = await Sock.ReceiveAsync(handshakeBuffer, SocketFlags.None, cts);
            }
            catch (SocketException ex)
            {
                Logger.Log($"Socket Exception Method: {ex.Message}, removing peer.");
                tracker.Peers.Remove(this);
                return false;
            }

            handshakeRecieved = handshakeRecieved == -1 ? 0 : handshakeRecieved;

            if (handshakeRecieved == 0)
            {
                Logger.Log($"Peer {Ip}:{Port} responded with 0 bytes, Continuing.", source: "Peer Handshake");
                tracker.Peers.Remove(this);
                return false;
            }

            Array.Resize(ref handshakeBuffer, handshakeRecieved);

            var asStr = Encoding.ASCII.GetString(handshakeBuffer);
            Logger.Log($"Handshake Response: {asStr}", source: "Peer Handshake");

            // TODO: message error handling, maybe an IError interface on all of them to signal an error with a boolean

            // see if recieved the entire 
            if (!(handshakeBuffer[0] + 49 == handshakeRecieved && Encoding.ASCII.GetString(handshakeBuffer.ReadBytes(1, handshakeBuffer[0])) == "BitTorrent protocol"))
            {
                Logger.Log($"Parsing went wrong!", source: "Peer Handshake");
                tracker.Peers.Remove(this);
                return false;
            }

            Logger.Log($"Handshake recieved correctly from {this}", source: "Peer Handshake");
            Logger.Log($"Now able to init download");
            return true;
        }

        public async Task<bool> SendBitField()
        {
            byte[] bitfield = tracker.Torrent.Bitfield.GetAvailabilityBitField();
            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                try
                {
                    await Sock.SendAsync(new ArraySegment<byte>(bitfield), SocketFlags.None, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    Logger.Log($"Bitfield was not received.", source: $"Peer: {this}, Tracker: {tracker}");
                    tracker.DestroyPeer(this);
                    return false;
                }
                catch (SocketException ex)
                {
                    Logger.Log($"Socket exception on bitfield message, {ex.Message}", source: $"Peer: {this}, Tracker: {tracker}");
                    tracker.DestroyPeer(this);
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Log($"Unexpected Error {ex.Message}");
                    tracker.DestroyPeer(this);
                    return false;
                }
            }
            Logger.Log($"Sent bitfield successfully!", source: $"Peer: {this}, Tracker: {tracker}");
            return true;
        }

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
            foreach (byte b in pstr) { data.Add(b); }

            // reserved
            for (int i = 0; i < 8; i++) { data.Add(0); }

            var hash = Encoding.ASCII.GetBytes(InfoHash);
            for (int i = 0; i < hash.Length; i++) { data.Add(hash[i]); }

            var peer_id = TrackerMessageHelpers.GenerateID();
            foreach (byte b in peer_id) { data.Add(b); }

            return data.ToArray();
        }

        public bool Equals(Peer? other)
        {
            return other != null && other.Ip == Ip;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Peer)
            {
                var other = (Peer)obj;
                return other.Ip == this.Ip;
            }
            return false;
        }
    }
}
