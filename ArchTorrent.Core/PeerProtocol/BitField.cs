using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.PeerProtocol
{
    public class TorrentBitField
    {
        /// <summary>
        /// byte representation of the bitfield, edited by helper functions for simplicity of the peer objects.
        /// </summary>
        private byte[] _data;

        public byte[] Bitfield { get => _data; }

        /// <summary>
        /// Currently downloading pieces, for reasons such as corruption or any socket error, I'll leave-
        /// the pieces that are downloading as 0, such that if a piece errors out, it will remain 0 and be overidden-
        /// by the peer and still be asked for (should an error occur, the peer is responsible for deleting the integer off the list)
        /// In the event of an ongoing download, the getter will return the piece with a 1, so no peer downloads the same piece.
        /// The hashset is used instead of a list so that should a duplicate arrive, an exception can be raised, which will-
        /// reset the peer.
        /// </summary>
        private HashSet<int> ongoingDownloads;

        private readonly object _lock = new object();

        public TorrentBitField(int size)
        {
            _data = new byte[size];
            ongoingDownloads = new();
        }

        /// <summary>
        /// Returns an altered version of the bitfield, which sets pieces that are currently downloading as 1,
        /// such that no peer will download the same piece twice (in theory).
        /// </summary>
        /// <returns></returns>
        public byte[] GetAvailabilityBitField()
        {
            // creates a deep copy of the 
            byte[] copy = new byte[_data.Length];
            Array.Copy(_data, copy, _data.Length);

            // set each ongoing download piece as 1 and return the copy.
            foreach (int pieceIndex in ongoingDownloads)
            {
                int byteIndex = pieceIndex / 8;
                int bitIndex = pieceIndex % 8;
                byte mask = (byte)(1 << bitIndex);
                copy[byteIndex] |= mask;
            }

            return copy;
        }


        /// <summary>
        /// this function adds a piece index to a hashset, it returns whether it is a duplicate,
        /// in which case an error has occured and the peer should reset.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool NotifyDownloadStarted(int index)
        {
            lock (_lock)
            {
                bool indexPresent = ongoingDownloads.Add(index);
                if (indexPresent)
                {
                    Logger.Log($"Index {index} is already present in the hashset, sending to peer.");
                }
                return !indexPresent;
            }
        }

        public bool NotifyDownloadFinished(int index)
        {
            lock (_lock)
            {
                bool indexPresent = ongoingDownloads.Remove(index);
                setBitfieldBit(index, true);
                if (!indexPresent)
                {
                    Logger.Log($"attempted to remove piece #{index} unsuccessfuly, set bitfield successful, no number released from ongoingDownloads.");
                }

                return indexPresent;
            }
        }

        /// <summary>
        /// During debug, this function will raise an uncaught exception, this is for debugging any possible out of bounds
        /// Yet to decide exactly how ArchTorrent should handle this sort of error, but for now, it will return false in production
        /// this is going to be changed as either a "corrupted torrent" message or some other type of error handling.
        /// In theory, this should never occur post debug, but those are famous last words.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool setBitfieldBit(int index, bool value)
        {
            lock (_lock)
            {
                int byteIndex = index / 8;
                int bitIndex = index % 8;
                byte mask = (byte)(1 << bitIndex);
                try
                {
                    if (value)
                        _data[byteIndex] |= mask;
                    else
                        _data[byteIndex] &= (byte)~mask;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Logger.Log($"Bitfield error: tried accessing bit {bitIndex} in byte {byteIndex} out of an array size of {_data.Length} (provided index: {index}", source: "Bitfield Accessor");
#if DEBUG
                    throw e;
#else
                return false;
#endif
                }
                return true;
            }
        }
    }
}
