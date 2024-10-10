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
using ArchTorrent.Core.PeerProtocol;

namespace ArchTorrent.Core.Torrents
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Torrent
    {
        [JsonProperty]
        public string FullFilePath { get; set; }

        internal static PortList PortList = new();

        #region Mandatory Fields

        [JsonProperty]
        public TorrentInfo Info { get; set; }

        [JsonProperty]
        public string AnnounceURL { get; set; }

        [JsonProperty]
        public long Uploaded { get; set; } = 0;

        [JsonProperty]
        public long Downloaded { get; set; } = 0;

        public long Left { get => Info.Size - Downloaded; }

        [JsonProperty]
        public List<Tracker> Trackers { get; set; } = new List<Tracker>();

        [JsonProperty]
        public TorrentBitField Bitfield { get; set; }

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

        public byte[] InfoHashBytes { get => TorrentUtil.CalculateInfoHashBytes(Info.OriginalDictionary); }

        // TODO: While I doubt it, this is potentially a slow hazard and needs to be benchmarked
        // at the time of writing, I can't be sure 
        public List<Peer> Peers { get
            {
                var p = new List<Peer>();
                Trackers.ForEach(x => p.AddRange(x.Peers));
                return p;
            } }

        /// <summary>
        /// creates a Torrent skeleton until .Read() is called
        /// </summary>
        /// <param name="fullPath">full path to the torrent</param>
        private Torrent(string fullPath, IEnumerable<string> announces)
        {
            FullFilePath = fullPath;
            foreach(string announce in announces)
            {
                if (announce.StartsWith("http"))
                    Trackers.Add(new HttpTracker(this, announce));
                else if(announce.StartsWith("udp"))
                    Trackers.Add(new UdpTracker(this, announce));
                else
                {
                    Logger.Log($"Invalid announce URI in torrent: {announce}. Skipping.", Logger.LogLevel.ERROR , source: "Torrent Constructor");
                }
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
            List<Tracker> toDel = new();

            foreach(var tracker in Trackers)
            {
                if (tracker.Peers.Count == 0) toDel.Add(tracker);
            }
            Logger.Log($"Removing all elements that did not respond to the tracker requests ({toDel.Count} out of {Trackers.Count}). Remaining: {toDel.Count - Trackers.Count}");
            toDel.ForEach(x => Trackers.Remove(x));

            var a = new List<List<Peer>>(b);
            Logger.Log($"[!] List of peers:", source:"GetPeers");
            a.ForEach(x => x.ForEach(y => Logger.Log($"PEER: {y}")));

            return Trackers.Count > 0;
        }

        /// <summary>
        /// deletes a peer from the peer list in 
        /// </summary>
        /// <param name="peer"></param>
        /// <returns></returns>
        public void DestroyPeer(Peer peer)
        {
            foreach(Tracker t in Trackers)
            {
                if(t.Peers.Any(x => x.Ip == peer.Ip))
                {
                    t.Peers.Remove(peer);
                }
            }
        }
        public async Task<bool> InitDownload()
        {
            await GetPeers();
            Logger.Log("Begun Download");
            return true;
        }

        // TODO: implement
        public static Torrent BCNetDictToATTorrent(BDictionary dict, string filePath = "")
        {
            // mandatory
            BDictionary info = dict.Get<BDictionary>("info");
            string announceURL = dict.Get<BString>("announce").ToString();
            BList? announceList = dict.Get<BList>("announce-list");

            List<string> announces = new List<string>
            {
                announceURL
            };
            if (announceList != null)
            {
                foreach(var t in announceList)
                {
                    BList? announce = t as BList;
                    if (announce == null) continue;

                    foreach(var bstr in announce)
                    {
                        BString? aStr = bstr as BString;
                        if (aStr == null) continue;
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
