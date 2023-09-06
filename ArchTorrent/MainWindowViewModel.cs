using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Torrent { get; set; } = "";

        public MainWindowViewModel()
        {

            Torrent = @"D:\NewRepos\ArchTorrent\ArchTorrent\sample.torrent";
            string file = File.ReadAllText(Torrent);
            Logger.Log(file);
        }
    }
}
