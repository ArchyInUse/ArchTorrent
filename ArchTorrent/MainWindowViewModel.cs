using ArchTorrent.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BCParsing = BencodeNET.Parsing;
using BCObjects = BencodeNET.Objects;
using System.IO.Pipelines;
using System.Collections.ObjectModel;
using ArchTorrent.Core.Torrents;
using Newtonsoft.Json;
using System.Net;

namespace ArchTorrent
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// TOOD: delete this as all torrents will be added through Torrent type then bound with Torrent.FullPath
        /// </summary>
        public ObservableCollection<string> TorrentPaths { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<Torrent> Torrents { get; set; } = new ObservableCollection<Torrent>();
        public TorrentClient Client { get; set; }

        public MainWindowViewModel()
        {
            TorrentPaths.Add(@"D:\NewRepos\ArchTorrent\ArchTorrent\sample.torrent");
            Logger.Log("STARTING LOG");

            var tmp = ReadTorrentFile(TorrentPaths[0]);

            Logger.Log("Read torrents successfully");

            Logger.Log("Constructing Torrent...");
            Torrent torrent = Torrent.BCNetDictToATTorrent(tmp, TorrentPaths[0]);
            Logger.Log("Completed Construction, outputting to file test.txt");

            using (StreamWriter sw = new StreamWriter("test.txt"))
            {
                string asStr = JsonConvert.SerializeObject(torrent);
                sw.Write(asStr);
            }
            //string str = "8:announce";
            //string integer = "i52481e";
            //Logger.Log(BencodeObject.DecodeString(str).ToString());
            //Logger.Log(BencodeObject.DecodeInteger(integer).ToString());
        }

        /// <summary>
        /// returns BCObject.BDictionary, needs to be turned to ArchTorrent.Core.Torrent
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private BCObjects.BDictionary ReadTorrentFile(string path)
        {
            Logger.Log("Begin readTorrentAsync");
            BCObjects.BDictionary ret;
            using (FileStream fs = File.OpenRead(path))
            {
                PipeReader pipeReader = PipeReader.Create(fs);
                var bencodeReader = new BencodeNET.IO.BencodeReader(fs);
                var parser = new BCParsing.BencodeParser();

                // TODO: CRASHES!
                ret = parser.Parse<BCObjects.BDictionary>(bencodeReader);
            }
            return ret;
        }

        private async Task<List<Torrent>> getLoadedTorrents(ICollection<string> paths)
        {
            List<Task<BCObjects.BDictionary>> tasks = new List<Task<BCObjects.BDictionary>>();
            foreach(string path in paths)
            {
                tasks.Add(Task.Run<BCObjects.BDictionary>(() => ReadTorrentFile(path)));
            }

            List<BCObjects.BDictionary> dictionaries = new (await Task.WhenAll(tasks));

            // parse and change here

            var b = dictionaries[0];

            return null;
        }
    }
}
