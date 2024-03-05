using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.Trackers.UDPTrackerProtocol
{
    internal class UdpMessageManager : IDisposable
    {
        public List<Socket> sockets { get; set; }
        public Dictionary<byte[], Func<byte[]>> callbacks { get; set; }
        public bool Receiving = false;

        /// <summary>
        /// port ranges
        /// </summary>
        private int _min, _max;
        
        public UdpMessageManager(int minPort = 6881, int maxPort = 6889)
        {
            sockets = new List<Socket>();
            callbacks = new Dictionary<byte[], Func<byte[]>>();
            _min = minPort;
            _max = maxPort;
        }

        public void Start()
        {
            if (Receiving) return;

            for(int i = _min; i <= _max; i++)
            {
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                sock.Bind(new IPEndPoint(IPAddress.Any, i));
            }

            Receiving = true;
        }
        public void Stop()
        {
            if(!Receiving) return;
            sockets.ForEach(socket => { socket.Close(); socket.Dispose(); });
            sockets = new();
            Receiving = false;
        }

        public void WaitForResponse(byte[] t_id, Func<byte[]> callback)
        {
            if(!Receiving)
            {
                throw new InvalidOperationException($"[-] Tried to wait for a response for t_id {t_id} while UdpMessageManager is not listening");
            }
            callbacks.Add(t_id, callback);
        }

        public void Dispose()
        {
            foreach(Socket sock in sockets)
            {
                sock.Close();
                sock.Dispose();
            }
        }
    }
}
