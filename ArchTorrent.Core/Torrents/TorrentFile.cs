using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IO = System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ArchTorrent.Core.Torrents
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TorrentFile
    {
        [JsonProperty]
        public long Length { get; set; }
        private string name;

        private string path;
        public string Md5sum { get; set; }

        [JsonProperty]
        public string Path { get => IO.Path.Combine(path, name); }

        public TorrentFile(string name, long length, string path = "", string? md5sum = null)
        {
            Length = length;
            this.name = name;
            this.path = path;
            Md5sum = md5sum ?? "";
        }
    }
}
