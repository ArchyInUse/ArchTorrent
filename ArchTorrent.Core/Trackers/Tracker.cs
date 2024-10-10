using ArchTorrent.Core.PeerProtocol;
using ArchTorrent.Core.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.Trackers
{
    public abstract class Tracker
    {
        public string AnnounceUrl { get; set; }
        public Uri AnnounceURI { get; set; }
        public Torrent Torrent { get; set; }
        public List<Peer> Peers { get; set; } = new List<Peer>();
        public bool DestroyPeer(Peer peer)
        {
            peer.Sock.Close();
            if(!Peers.Contains(peer))
            {
                Logger.Log($"Tried to destroy peer that is not present in the Peers list, tracker: {this}");
                return false;
            }
            return Peers.Remove(peer);
        }

        public abstract Task<List<Peer>> TryGetPeers();

        public override string ToString() => AnnounceUrl;
    }
}
