using ArchTorrent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ArchTorrent.Core.Torrents;
using System.Text.Json.Serialization;
using System.Collections;

namespace ArchTorrent.Cache
{
    /// <summary>
    /// The ArchTorrent cache, saves all torrent locations
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ATCache : IEnumerable<string>
    {
        [JsonProperty]
        public List<string> TorrentPaths { get; set; }

        [JsonProperty]
        public bool Empty = false;
        public const string CachePath = @"%appdata%\ArchTorrent\Cache.json";

        public async static Task<ATCache?> GetCache()
        {
            ATCache? cache = new();
            
            // we don't save the cache without content, so the GetCache would always work.
            // unless Save() is called, there is no reason to store the cache empty
            if (!File.Exists(CachePath))
            {
                cache.Empty = true;
                return null;
            }

            string json = await File.ReadAllTextAsync(CachePath);
            cache = JsonConvert.DeserializeObject<ATCache>(json);
            if(cache == null)
            {
                cache = new ATCache();
                return cache;
            }

            foreach(var b in cache)
            {
                if(!File.Exists(b))
                {
                    cache.TorrentPaths.Remove(b);
                }
            }
            return cache;
        }

        public ATCache(IEnumerable<string> paths)
        {
            if(paths.Count() == 0)
            {
                Empty = true;
            }
            TorrentPaths = paths.ToList();
        }

        public void ForEach(Action<string> action) => TorrentPaths.ForEach(action);

        public async Task Save()
        {
            var json = JsonConvert.SerializeObject(TorrentPaths);
            await File.WriteAllTextAsync(CachePath, json);
        }

        public IEnumerator<string> GetEnumerator() => TorrentPaths.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => TorrentPaths.GetEnumerator();
    }
}
