using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.Trackers
{
    public class Peer
    {
        public IPAddress Ip { get; set; }
        public int Port { get; set; }

        public Peer(Int32 ip, Int16 port)
        {
            Ip = new IPAddress(ip);
            Port = port;
        }

        public override string ToString()
        {
            return $"Peer: {Ip}:{Port}";
        }
    }
}
