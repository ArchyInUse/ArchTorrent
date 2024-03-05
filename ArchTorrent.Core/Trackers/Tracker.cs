using ArchTorrent.Core.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.Trackers
{
    public class Tracker
    {
        public string AnnounceUrl { get; set; }
        public Uri AnnounceURI { get; set; }
        public Torrent Torrent { get; set; }
    }
}
