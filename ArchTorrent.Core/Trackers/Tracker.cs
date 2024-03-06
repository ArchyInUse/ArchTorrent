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

        public abstract Task<List<Peer>> TryGetPeers();
    }
}
