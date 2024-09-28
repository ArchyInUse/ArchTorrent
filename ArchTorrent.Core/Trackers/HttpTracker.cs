using ArchTorrent.Core.PeerProtocol;
using ArchTorrent.Core.Torrents;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArchTorrent.Core.Trackers
{
    public class HttpTracker : Tracker
    {
        private bool SSL;
        private readonly HttpClient _httpClient;
        private readonly int listenPort = 6890;

        /// <summary>
        /// interval between regular requests to tracker.
        /// </summary>
        private int messageInterval = -1;

        /// <summary>
        /// minimum announce interval.
        /// </summary>
        private int minAnnounceInterval = -1;

        public int Seeders { get; set; } = 0;
        public int Leechers { get; set; } = 0;
        public int PeerCount { get => Peers.Count; }

        public HttpTracker(Torrent torrent, string announceURL) 
        {
            Torrent = torrent;
            AnnounceUrl = announceURL;
            AnnounceURI = new Uri(announceURL);
            SSL = AnnounceUrl.ToLower().StartsWith("https");
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.All
            };
            _httpClient = new HttpClient(handler);
            Peers = new List<Peer>();
        }

        /// <summary>
        /// get peers from HTTP tracker.
        /// </summary>
        /// <returns>returns empty list on failure.</returns>
        public override async Task<List<Peer>> TryGetPeers()
        {
            /// parameters:
            /// info_hash (url encoded bytes)
            /// peer_id
            /// port => 6890
            /// uploaded (amount of uploaded bytes)
            /// downloaded
            /// left
            /// compact
            /// no_peer_id
            /// event
            ///     started
            ///     stopped
            ///     completed
            /// ip (optional)
            /// numwant (optional)
            /// key (optional) an additional identification
            /// trackerid (optional)
            Dictionary<string, string> parameters = new();
            
            string urlEncodedInfoHash = HttpUtility.UrlEncode(Torrent.InfoHashBytes);
            parameters.Add("info_hash", urlEncodedInfoHash);

            // string infoHashUrlEncoded = WebUtility.UrlEncode(infoHash);
            string peerId = TrackerMessageHelpers.ATVERSION;
            parameters.Add("peer_id", peerId);

            // note: listening port will always be the same, no variable needed.
            long uploaded = Torrent.Uploaded;
            long downloaded = Torrent.Downloaded;
            long left = Torrent.Left;
            parameters.Add("port", "6890");
            parameters.Add("uploaded", uploaded.ToString());
            parameters.Add("downloaded", downloaded.ToString());
            parameters.Add("left", left.ToString());

            // compact responses are not difficult to parse, and some trackers only accept compact response.
            // therefore I've decided to exclusively ask for compact responses.
            parameters.Add("compact", "1");

            // getting peers must start with a "started" event, 
            string trackerEvent = "started";
            parameters.Add("event", trackerEvent);

            // ip - used when using any proxy, as a hobby project I will ignore this.
            // numwant - optional, the default is (typically) 50 peers, which is enough.
            // key - additional authentication, currently no reason to implement.
            // trackerid - This is only prevelant in the case that an announce contained a trackerid.
            string response = await GetAsync(AnnounceUrl, parameters);

            var parser = new BencodeParser();
            BDictionary responseDictionary = parser.ParseString<BDictionary>(response);
            if(responseDictionary.ContainsKey("failure reason"))
            {
                Logger.Log($"Tracker response contains failure message: {responseDictionary["failure reason"]}");
                Logger.Log($"Returning empty list.");
                return new();
            }
            else if(responseDictionary.ContainsKey("warning message"))
            {
                // as per the torrent protocol standard, warnings do not count as failure, we can continue as normal.
                Logger.Log($"Tracker response contains warning: {responseDictionary["warning message"]}");
            }
            BNumber interval = (BNumber)responseDictionary["interval"];
            BNumber minInterval = (BNumber)responseDictionary["min interval"];
            BString trackerId = (BString)responseDictionary["tracker id"];
            BNumber seeders = (BNumber)responseDictionary["complete"];
            BNumber leechers = (BNumber)responseDictionary["incomplete"];

            // create return value
            List<Peer> peers = new();

            // tracker responses can either be a BDictionary with the peers, or the binary model.
            if (responseDictionary["peers"].GetType() == typeof(BDictionary))
            {
                BDictionary bencodePeers = (BDictionary)responseDictionary["peers"];
            }
            else
            {
                BString bencodePeers = (BString)responseDictionary["peers"];
                byte[] bytePeers = bencodePeers.Value.ToArray();
                if(bytePeers.Length % 6 != 0)
                {
                    Logger.Log($"Byte peer array length is not divisible by 6!", source: "HTTP Tracker");
                    Logger.Log($"Returning empty list and printing byte array (hex):", source: "HTTP Tracker");
                    if (bytePeers.Length == 0) Logger.Log("Empty Array");
                    else
                    {
                        Logger.Log(Convert.ToHexString(bytePeers));
                    }
                    return new();
                }
                
                for(int i = 0; i < bytePeers.Length / 6; i += 6)
                {
                    byte[] ip = bytePeers[i..(i + 4)];
                    byte[] port = bytePeers[(i + 4)..(i + 6)];
                    peers.Add(new Peer(ip, port, Torrent.InfoHash, this));
                }
                return peers;
            }


            return new();
        }

        /// <summary>
        /// Makes a get request with the specified parameters dictionary
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private async Task<string> GetAsync(string uri, Dictionary<string, string> parameters)
        {
            // build query parameters
            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach(var pair in parameters)
            {
                query[pair.Key] = pair.Value;
            }
            // build uri and add parameters
            var builder = new UriBuilder(uri);
            builder.Query = query.ToString();

            // make request
            using HttpResponseMessage response = await _httpClient.GetAsync(builder.ToString());
            return await response.Content.ReadAsStringAsync();
        }
    }
}
