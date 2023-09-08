using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Parsing;

namespace ArchTorrent.Core.Trackers
{
    public class UdpTracker : Tracker
    {

        private Task udpSendAsync()
        {
            BencodeParser parser = new BencodeParser();
            var t = parser.Parse<BencodeNET.Torrents.Torrent>(@"D:\NewRepos\ArchTorrent\ArchTorrent\sample.torrent");
            

            
        }
    }
}
