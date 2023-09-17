using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Parsing;

namespace ArchTorrent.Core.Trackers
{
    public class UdpTracker : Tracker
    {
        public string AnnounceUrl { get; set; }
        public Uri AnnounceURI { get => new Uri(AnnounceUrl, UriKind.Absolute); }
        public UdpTracker(string announceURL)
        {
            AnnounceUrl = announceURL;
        }

        public async Task SendAsync(string content)
        {
            //Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //IPHostEntry hostInfo = Dns.Resolve(Dns.GetHostEntry(AnnounceURI.))
        }


    }
}
