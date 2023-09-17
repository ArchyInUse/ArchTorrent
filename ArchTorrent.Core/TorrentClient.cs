using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ArchTorrent.Core.Torrents;

namespace ArchTorrent.Core
{
    public class TorrentClient
    {
        Torrent torrent { get; set; }   
        public TorrentClient(Torrent torrent)
        {
            this.torrent = torrent; 
        }
        /// <summary>
        /// Test ctor, DO NOT USE
        /// </summary>
        public TorrentClient()
        {
                
        }
        public bool TestSendUDPMessageAsync(string content)
        {
            UdpClient udpClient = new ();
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            try
            {
                //udpClient.BeginSend(buffer, buffer.Length, new IPEndPoint("www.");
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Logger.LogLevel.ERROR);
            }

            return true;
        }
    }
}
