using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Models
{
    public class DebugWindowModel : INotifyPropertyChanged
    {
        #region INPC

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public string DebugText { get => _debugText; set
            {
                _debugText = value;
                NotifyPropertyChanged(nameof(DebugText));
            }
        }
        private string _debugText = string.Empty;
    }
}
