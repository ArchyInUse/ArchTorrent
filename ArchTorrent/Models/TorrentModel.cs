using ArchTorrent.Core;
using ArchTorrent.Core.Torrents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ArchTorrent.Models
{
    public class TorrentModel : INotifyPropertyChanged
    {
        #region INPC

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public Torrent Torrent { get; set; }

        public long DownloadedValue { get; set; }
        public long TotalSize { get => Torrent.Info.Size; }

        public TorrentModel(Torrent torrent)
        {
            Torrent = torrent;
            StartWatching();
        }

        private CancellationTokenSource updateCTS = null;
        private Task updateWatcher;

        private async Task StartWatching()
        {
            if(updateCTS != null)
            {
                Logger.Log("[CRITICAL] CancellationToken already exists, running StopWatching", source: "TorrentModel UpdateWatcher");
                await StopWatching();
            }

            updateCTS = new CancellationTokenSource();
            try
            {
                await Task.Run(async () =>
                {
                    while (!updateCTS.Token.IsCancellationRequested)
                    {
                        // This must be dispatched back to the UI thread to update UI elements safely
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            UpdateUI();
                        });

                        await Task.Delay(1000); // Delay for 1 second
                    }
                }, updateCTS.Token);
            }
            catch (OperationCanceledException)
            {
                // Handle the task cancellation
                Logger.Log("Stopped updating UI by request.");
            }
        }

        private void UpdateUI()
        {
            // TODO: implement UI elements that need to be checked as observables
        }

        private async Task StopWatching()
        {
            updateCTS.Cancel();
            await Task.Delay(1500);
        }
    }
}
