using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.Torrents
{
    public enum TorrentDownloadState
    {
        Paused,
        Downloading,
        Seeding,
    }
}
