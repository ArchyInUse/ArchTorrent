using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDictionary = BencodeNET.Objects.BDictionary;
using BString = BencodeNET.Objects.BString;
using BList = BencodeNET.Objects.BList;
using BInteger = BencodeNET.Objects.BNumber;
using Newtonsoft.Json;

namespace ArchTorrent.Core.Torrents
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TorrentInfo
    {
        #region Mandatory
        [JsonProperty]
        public long PieceLength { get; private set; }

        // not serializable as bytes
        public byte[] Pieces { get; private set; }

        [JsonProperty]
        public string DirectoryName { get; private set; }

        [JsonProperty]
        public List<TorrentFile> Files { get; private set; } = new List<TorrentFile>();

        #endregion

        #region Optional

        [JsonProperty]
        public bool Private { get; private set; } = false;

        #endregion

        [JsonProperty]
        public bool SingleFile { get; private set; } = false;

        public BDictionary OriginalDictionary { get; private set; }

        public TorrentInfo(BDictionary infoDict)
        {
            OriginalDictionary = infoDict;
            // single file mode
            if (infoDict.Get<BList>("files") == null)
            {
                SingleFile = true;
                string filename = infoDict.Get<BString>("name").ToString();
                long length = infoDict.Get<BInteger>("piece length").Value;
                var bMd5sum = infoDict.Get<BString>("md5sum");
                string? md5sum = "";
                if(bMd5sum != null) md5sum = bMd5sum.ToString();
                Files.Add(new TorrentFile(filename, length, md5sum));
            }
            // multi file mode
            else
            {
                string dirName = infoDict.Get<BString>("name").ToString();
                BList filesList = infoDict.Get<BList>("files");
                foreach(var b in filesList)
                {
                    BDictionary asDict = b as BDictionary ?? throw new NullReferenceException("Incorrect torrent format");
                    BList paths = asDict.Get<BList>("path");
                    long length = asDict.Get<BInteger>("length").Value;
                    var bMd5sum = asDict.Get<BString>("md5sum");

                    string md5sum = bMd5sum != null ? bMd5sum.ToString() : string.Empty;

                    List<string> pathsList = new List<string>();
                    foreach(var p in paths)
                    {
                        BString asStr = p as BString ?? throw new NullReferenceException("Paths returned null");
                        pathsList.Add(asStr.ToString());
                    }
                    var fileName = pathsList.Last();
                    // remove last element as it's the file name
                    pathsList.RemoveAt(pathsList.Count - 1);
                    Files.Add(new TorrentFile(fileName, length, Path.Combine(pathsList.ToArray()), md5sum));
                }
                DirectoryName = dirName;
            }

            // torrent is not single file
            PieceLength = infoDict.Get<BInteger>("piece length").Value;
            Pieces = infoDict.Get<BString>("pieces").Value.ToArray();
            if (infoDict.Get<BInteger>("private").Value != 0) Private = true;

        }
    }
}
