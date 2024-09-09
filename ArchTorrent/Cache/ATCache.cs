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
using System.Runtime.CompilerServices;
using ArchTorrent.Core;

namespace ArchTorrent.Cache
{
    /// <summary>
    /// The ArchTorrent cache, saves all torrent locations
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ATCache : IEnumerable<string>
    {
        [JsonProperty]
        public List<string> infoHashes { get; set; } = new List<string>();

        [JsonProperty]
        public Dictionary<string, Torrent> Torrents { get; set; } = new();

        [JsonProperty]
        private static string defaultPath = @"%appdata%\ArchTorrent\Cache.json";

        public static ATCache Instance { get; set; }

        public ATCache() { }

        /// <summary>
        /// Returns null if a cache is not found.
        /// </summary>
        /// <param name="uniquePath"></param>
        /// <returns></returns>
        public async static Task<ATCache?> Load(string uniquePath = "")
        {
            string Path = uniquePath == "" ? uniquePath : defaultPath;
            if(!File.Exists(Path))
            {
                Logger.Log("Cache not found. Creating a new cache with default settings.");

                return new ATCache();
            }

            string cache = await File.ReadAllTextAsync(Path);
            ATCache? loadedCache;
            try
            {
                loadedCache = JsonConvert.DeserializeObject<ATCache>(cache);
            }
            catch(Exception ex)
            {
                Logger.Log($"Error loading cache: {ex.Message}, returning null cache.");
                return null;
            }
            
            if(loadedCache == null)
            {
                Logger.Log("Cache deserialization failed. Returning null cache.");
                return null;
            }

            return loadedCache;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return infoHashes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return infoHashes.GetEnumerator();
        }
    } 
}
