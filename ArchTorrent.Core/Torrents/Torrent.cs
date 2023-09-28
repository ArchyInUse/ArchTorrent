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
using BencodeNET.Torrents;
using ArchTorrent.Core.Trackers;
using System.Net;
using System.ComponentModel;

namespace ArchTorrent.Core.Torrents
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Torrent
    {
        [JsonProperty]
        public string FullFilePath { get; set; }

        internal static PortList PortList = new PortList();

        #region Mandatory Fields

        [JsonProperty]
        public TorrentInfo Info { get; set; }

        [JsonProperty]
        public string AnnounceURL { get; set; }

        // TODO: change this so it can be both UDP & HTTP
        [JsonProperty]
        public List<UdpTracker> Trackers { get; set; } = new List<UdpTracker>();

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

        public string InfoHash { get => TorrentUtil.CalculateInfoHash(Info.OriginalDictionary); }

        /// <summary>
        /// creates a Torrent skeleton until .Read() is called
        /// </summary>
        /// <param name="fullPath">full path to the torrent</param>
        private Torrent(string fullPath, IEnumerable<string> announces)
        {
            FullFilePath = fullPath;
            foreach(string announce in announces)
            {
                Trackers.Add(new UdpTracker(this, announce));
            }
        }

        // TODO: RecieveAsync hangs, therefor this cannot be fully asynchronous you have to wait for AsyncResult.WaitOne
        // some change needs to be done to ExecuteUdpRequest for it to work 
        public async Task<bool> GetPeers()
        {
            if (Trackers.Count == 0) return false;

            var tasks = new List<Task<List<Peer>>>();
            
            for(int i = 0; i < Trackers.Count - 1; i++)
            {
                tasks.Add(Trackers[i].TryGetPeers());
                //tasks.Add(Task.Run(() => Trackers[i].TryGetPeers()));
            }
            Logger.Log($"Amount of tasks: {tasks.Count}");
            var b = await Task.WhenAll(tasks);
            Logger.Log($"Completed all {tasks.Count} tasks!");
            List<UdpTracker> toDel = new();

            foreach(var tracker in Trackers)
            {
                if (tracker.Peers.Count == 0) toDel.Add(tracker);
            }
            Logger.Log($"Removing all elements that did not respond to the tracker requests ({toDel.Count} out of {Trackers.Count}). Remaining: {toDel.Count - Trackers.Count}");
            toDel.ForEach(x => Trackers.Remove(x));

            var a = new List<List<Peer>>(b);
            a.ForEach(x => x.ForEach(y => Logger.Log($"PEER: {y}")));

            return Trackers.Count > 0;
        }

        // TODO: implement
        public static Torrent BCNetDictToATTorrent(BDictionary dict, string filePath = "")
        {
            // mandatory
            BDictionary info = dict.Get<BDictionary>("info");
            string announceURL = dict.Get<BString>("announce").ToString();
            BList? announceList = dict.Get<BList>("announce-list");

            List<string> announces = new List<string>();
            announces.Add(announceURL);
            if(announceList != null)
            {
                foreach(var t in announceList)
                {
                    BList? announce = t as BList;
                    if (announce == null) continue;

                    foreach(var bstr in announce)
                    {
                        BString? aStr = bstr as BString;
                        if (aStr == null) continue;
                        if (!aStr.ToString().StartsWith("udp")) continue;
                        Logger.Log($"Adding URL: {aStr}", source: "---Initialize Torrent---");
                        announces.Add(aStr.ToString());
                    }
                }
            }

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

            Torrent ret = new Torrent(filePath, announces);
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
