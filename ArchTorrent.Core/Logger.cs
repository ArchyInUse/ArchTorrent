using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace ArchTorrent.Core
{
    public static partial class Logger
    {
        public static string Log(string message, LogLevel severity = 0, string source = "main")
        {
            // currently not using the LogLevel system, but important to have a place to implement it
            // should it be needed.
            string m = $"[{DateTime.Now}] [{source}] " + message;
            AddToFile(m);
            Debug.WriteLine(m);

            return m;
        }

        private static bool AddToFile(string message, LogLevel severity = 0)
        {
            try
            {
                File.AppendAllText("log.txt", message);
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }
    }
}
