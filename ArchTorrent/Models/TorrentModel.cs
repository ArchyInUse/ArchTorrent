﻿using ArchTorrent.Core.Torrents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
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

        public long DownloadedValue { get; set; }
        public long TotalSize { get => Torrent.Info.Size; }

        public TorrentModel(Torrent torrent)
        {
            Torrent = torrent;
            
        }
    }
}
