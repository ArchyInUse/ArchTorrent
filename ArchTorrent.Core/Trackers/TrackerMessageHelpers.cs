using ArchTorrent.Core.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.Trackers
{
    internal static class TrackerMessageHelpers
    {
        /// <summary>
        /// ArchTorrent version to send to trackers
        /// </summary>
        public const string ATVERSION = "-AT0001-";

        #region Encode Decode
        /// <summary>
        /// returns 2 byte array in big endian order
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns></returns>
        public static byte[] EncodeInteger(this Int16 encoded)
        {
            if(BitConverter.IsLittleEndian)
            {
                return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(encoded));
            }
            return BitConverter.GetBytes(encoded);
        }

        /// <summary>
        /// returns 4 byte array in big endian order
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns></returns>
        public static byte[] EncodeInteger(this Int32 encoded)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(encoded));
            }
            return BitConverter.GetBytes(encoded);
        }

        /// <summary>
        /// returns 8 byte array in big endian order
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns></returns>
        public static byte[] EncodeInteger(this Int64 encoded)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(encoded));
            }
            return BitConverter.GetBytes(encoded);
        }

        /// <summary>
        /// Decodes 2 byte array to Int16 (short) from big endian
        /// </summary>
        /// <param name="decode"></param>
        /// <returns></returns>
        public static Int16 DecodeInt16(this byte[] decode)
        {
            if (BitConverter.IsLittleEndian)
            {
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(decode));
            }
            return BitConverter.ToInt16(decode);
        }

        /// <summary>
        /// Decodes 4 byte array to Int32 (int) from big endian
        /// </summary>
        /// <param name="decode"></param>
        /// <returns></returns>
        public static Int32 DecodeInt32(this byte[] decode)
        {
            if (BitConverter.IsLittleEndian)
            {
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(decode));
            }
            return BitConverter.ToInt16(decode);
        }

        /// <summary>
        /// Decodes 8 byte array to Int64 (long) from big endian
        /// </summary>
        /// <param name="decode"></param>
        /// <returns></returns>
        public static Int64 DecodeInt64(this byte[] decode)
        {
            if (BitConverter.IsLittleEndian)
            {
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(decode));
            }
            return BitConverter.ToInt16(decode);
        }

        #endregion

        #region Printing

        public static string HexToString(this byte[] data)
        {
            string ret = "[";

            // leave last for special formatting
            for (int i = 0; i < data.Length - 1; i++)
                ret += $"{data[i]:X2}, ";

            return ret + $"{data[^1]:X2} " + "]"; 
        }

        #endregion

        #region General

        /// <summary>
        /// returns a formatted (for example: '-AT0001-{...}') byte array that represents an ID (usually Peer id) 
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateID()
        {
            byte[] data = new byte[20];
            new Random().NextBytes(data);
            var atver = Encoding.ASCII.GetBytes(ATVERSION);
            Array.Copy(atver, data, atver.Length);
            return data;
        }

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

        public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeout)))
            {
                return await task;
            }

            throw new TimeoutException("ExecuteUDPRequest Timed out.");
        }

        #endregion
    }
}
