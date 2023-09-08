using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core
{
    public class Torrent
    {
        public string Path { get; set; }

        /// <summary>
        /// 1 mb of content available
        /// </summary>
        private byte[] Content = new byte[1048576];
        /// <summary>
        /// creates a Torrent skeleton until .Read() is called
        /// </summary>
        /// <param name="fullPath">full path to the torrent</param>
        public Torrent(string fullPath)
        {
            Path = fullPath;

            // TODO: decide wheather this call should be automatic or not
            if(Read())
            {
                Logger.Log("Read successful.");
            }
            else
            {
                Logger.Log("Read Unsuccessful", Logger.LogLevel.ERROR);
            }
        }

        private bool Read()
        {
            try
            {
                Content = File.ReadAllBytes(Path);
            }
            catch(Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
