using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArchTorrent.Core;
using ArchTorrent.Core.PeerProtocol;
using ArchTorrent.Core.Torrents;
using ArchTorrent.Core.Trackers;
using ArchTorrent.Lib;

namespace ArchTorrent
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        public Command TestDownload { get; set; }

        public void InitEvents()
        {
            TestDownload = new Command(InvokeTestDownload2);
        }

        public async void InvokeTestDownload2()
        {
            foreach(var t in Torrents)
            {
                t.GetPeers();
            }
        }

        public async void InvokeTestDownload()
        {
            Logger.Log($"Starting Test Download");
            CancellationToken cancellationToken = CancellationToken.None;
            //Torrents[0].Trackers.ForEach(tracker => tracker.Peers.ForEach(p => p.InitDownloadAsync(cancellationToken)));
            int total = Torrents[0].Trackers.Sum(x => x.Peers.Count);
            int i = 0;
            Task<bool>[] results = new Task<bool>[total];
            foreach (UdpTracker u in Torrents[0].Trackers)
            {
                foreach (Peer p in u.Peers)
                {
                    Logger.Log($"Testing peer {i}/{total}");
                    results[i] = p.HandshakePeer(cancellationToken);
                    i++;
                }
            }
            bool[] resultValues = await Task.WhenAll(results);

            int numTrue = resultValues.Count(x => x);
            Logger.Log($"Finished handshake sequence from {total} peers. {numTrue}/{total} returned true.");

        }
    }
}
