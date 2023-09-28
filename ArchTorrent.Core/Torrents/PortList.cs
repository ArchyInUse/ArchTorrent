using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.Torrents
{
    internal class PortList
    {
        private Dictionary<int, bool> _openPorts = new Dictionary<int, bool>();

        public PortList(int min = 6881, int max = 6889)
        {
            for (int i = min; i <= max; i++)
                _openPorts.Add(i, false);
        }

        /// <summary>
        /// returns an unused port. If there aren't any, returns -1
        /// </summary>
        /// <returns></returns>
        public int GetOpenPort()
        {
            if (_openPorts.All(p => p.Value)) return -1;
            var b = _openPorts.First(x => !x.Value);
            _openPorts[b.Key] = true;
            return b.Key;
        }

        public void SetPortUnused(int port) 
        {
            _openPorts[port] = false;
        }

        /// <summary>
        /// waits for an open port
        /// </summary>
        /// <returns></returns>
        public async Task<int> AwaitOpenPort()
        {
            int p = GetOpenPort();
            while(p == -1)
            {
                p = GetOpenPort();
                await Task.Delay(100);
            }
            return p;
        }
    }
}
