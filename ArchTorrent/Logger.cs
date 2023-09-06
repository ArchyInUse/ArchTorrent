using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ArchTorrent
{
    public static partial class Logger
    {
        public static void Log(string message, LogLevel severity = 0)
        {
            // currently not using the LogLevel system, but important to have a place to implement it
            // should it be needed.
            Debug.WriteLine($"[{DateTime.Now}] " + message);
        }
    }
}
