using ArchTorrent.Core.PeerProtocol;
using ArchTorrent.Core.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.Trackers
{
    public class HttpTracker : Tracker
    {
        private bool SSL;
        public HttpTracker(Torrent torrent, string announceURL) 
        {
            Torrent = torrent;
            AnnounceUrl = announceURL;
            AnnounceURI = new Uri(announceURL);
            SSL = AnnounceUrl.ToLower().StartsWith("https");
        }

        public override Task<List<Peer>> TryGetPeers()
        {
            /// parameters:
            /// info_hash (url encoded bytes)
            /// peer_id
            /// port
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


            // infohash as string
            string ih = Torrent.InfoHash;

            // credit: https://stackoverflow.com/a/321404/10337916
            byte[] infoHash = Enumerable.Range(0, ih.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(ih.Substring(x, 2), 16))
                .ToArray();

            string infoHashUrlEncoded = WebUtility.UrlEncode(infoHash);
        }

        /// <summary>
        /// this function assumes the parameters of the GET request are present in the URL and follows standard CGI methods
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<string> MakeGETRequest(string url)
        {

        }
    }
}
