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

        public static Int16 DecodeInt16(this byte[] decode)
        {
            if (BitConverter.IsLittleEndian)
            {
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(decode));
            }
            return BitConverter.ToInt16(decode);
        }

        public static Int32 DecodeInt32(this byte[] decode)
        {
            if (BitConverter.IsLittleEndian)
            {
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(decode));
            }
            return BitConverter.ToInt16(decode);
        }

        public static Int64 DecodeInt64(this byte[] decode)
        {
            if (BitConverter.IsLittleEndian)
            {
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(decode));
            }
            return BitConverter.ToInt16(decode);
        }
    }
}
