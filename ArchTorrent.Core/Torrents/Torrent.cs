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
    public class Torrent
    {
        [JsonProperty]
        public string FullFilePath { get; set; }

        #region Mandatory Fields

        [JsonProperty]
        public TorrentInfo Info { get; set; }

        [JsonProperty]
        public string AnnounceURL { get; set; }


        #endregion

        #region Optional Fields
        [JsonProperty]
        public DateTime CreationTime { get; set; } = DateTime.MinValue;

        [JsonProperty]
        public string Comment { get; set; } = "";

        [JsonProperty]
        public string Creator { get; set; } = "";

        [JsonProperty]
        public string Encoding { get; set; } = "";
        #endregion

        /// <summary>
        /// creates a Torrent skeleton until .Read() is called
        /// </summary>
        /// <param name="fullPath">full path to the torrent</param>
        private Torrent(string fullPath)
        {
            FullFilePath = fullPath;

        }

        // TODO: implement
        public static Torrent BCNetDictToATTorrent(BDictionary dict, string filePath = "")
        {
            // mandatory
            BDictionary info = dict.Get<BDictionary>("info");
            string announceURL = dict.Get<BString>("announce").ToString();

            // optional fields
            // announceList : list<list<string>>
            // currently ignored.
            // BList? announceList = dict.Get<BList>("announce-list");
            BInteger? creationDate = dict.Get<BInteger>("creation date") ?? 0;

            BString? bComment = dict.Get<BString>("comment");
            string comment = string.Empty;
            if (bComment != null) comment = bComment.ToString();

            BString? bCreated_by = dict.Get<BString>("created by");
            string created_by = string.Empty;
            if (bCreated_by != null) created_by = bCreated_by.ToString();

            BString? bEncoding = dict.Get<BString>("encoding");
            string encoding = string.Empty;
            if (bEncoding != null) encoding = bEncoding.ToString();

            Torrent ret = new Torrent(filePath);
            TorrentInfo torrentInfo = new TorrentInfo(info);
            ret.Info = torrentInfo;
            ret.AnnounceURL = announceURL; 
            ret.FullFilePath = filePath;

            if (creationDate != 0) ret.CreationTime = DateTime.FromBinary(creationDate.Value);
            if (comment != "") ret.Comment = comment;
            if (created_by != "") ret.Creator = created_by;
            if (encoding != "") ret.Encoding = encoding;

            return ret;
        }
    }
}
