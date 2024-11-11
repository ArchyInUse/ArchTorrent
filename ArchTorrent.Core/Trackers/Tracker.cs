using ArchTorrent.Core.PeerProtocol;
using ArchTorrent.Core.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Core.Trackers
{
    public abstract class Tracker
    {
        /// <summary>
        /// bitfield send threshhold represent the percentage that a bitfield should be sent to start seeding,
        /// if the amount of bytes downloaded is lower than the percent, the bitfield won't be sent to focus on downloading
        /// instead of uploading.
        /// </summary>
        private const int BitfieldThreshhold = 20;
        public string AnnounceUrl { get; set; }
        public Uri AnnounceURI { get; set; }
        public Torrent Torrent { get; set; }
        public List<Peer> Peers { get; set; } = new List<Peer>();
        public bool DestroyPeer(Peer peer)
        {
            peer.Sock.Close();
            if(!Peers.Contains(peer))
            {
                Logger.Log($"Tried to destroy peer that is not present in the Peers list, tracker: {this}");
                return false;
            }
            return Peers.Remove(peer);
        }

        public void Pause()
        {
            if(Peers.Count == 0) 
            {
                // list is empty, tracker is already paused.
                return;
            }
            // otherwise, destroy all peers, which stops the download.
            Peers.ForEach(peer => DestroyPeer(peer));
        }

        public async Task Start()
        {
            try
            {
                await TryGetPeers();
            }
            catch(Exception ex)
            {
                Logger.Log($"Start of tracker {this} failed, exception: {ex.Message}", source:"Tracker Start()");
            }
        }
        
        public abstract Task<List<Peer>> TryGetPeers();

        /// <summary>
        /// begins initialization on all peers
        /// </summary>
        /// <returns></returns>
        public async Task Init()
        {
            List<Task<bool>> tasks = new();
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            // initiate upload only if we've downloaded more than 20% of the file.
            // begin handshakes
            List<Task> handshakes = new();
            Logger.Log($"Starting handshakes on tracker {this}");
            foreach(var peer in Peers)
            {
                handshakes.Add(peer.HandshakePeer(cts.Token));
            }
            await Task.WhenAll(handshakes);

            foreach(var task in tasks)
            {
                if(!task.Result)
                {
                    DestroyPeer(Peers[tasks.IndexOf(task)]);
                }
            }
                
            // check for failed handshakes (destroy peer)
            if(Torrent.Downloaded > (Torrent.Info.Size * (BitfieldThreshhold * 0.01)))
            {
                tasks.Clear();
                foreach(var peer in Peers)
                {
                    tasks.Add(peer.SendBitField());
                }
                await Task.WhenAll(tasks);
            }
        }

        public override string ToString() => AnnounceUrl;
    }
}
