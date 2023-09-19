using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Torrents;

namespace ArchTorrent.Core.Trackers
{
    public static class TrackerUtils
    {
        /// <summary>
        /// reads the amount of data from the byte array and returns it
        /// </summary>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static byte[] ReadBytes(this byte[] source, int index, int amount)
        {
            byte[] res = new byte[amount];
            for (int i = index; i < (index + amount); i++)
            {
                res[i - index] = source[i];
            }
            return res;
        }

        /// <summary>
        /// ArchTorrent version to send to trackers
        /// </summary>
        public const string ATVERSION = "-AT0001-";
    }
}
