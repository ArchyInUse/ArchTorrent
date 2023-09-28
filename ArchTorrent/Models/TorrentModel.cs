using ArchTorrent.Core.Torrents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

        public long Downloaded { get; set; }
        public long TotalSize { get => Torrent.Info.Size; }

        public TorrentModel(Torrent torrent)
        {
            Torrent = torrent;
        }
    }
}
