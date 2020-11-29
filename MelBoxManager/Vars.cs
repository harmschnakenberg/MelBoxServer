using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static MelBoxGsm.GsmEventArgs;

namespace MelBoxManager
{


    public class Var : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private ObservableCollection<LogItem> _TrafficList = new ObservableCollection<LogItem>();

        public ObservableCollection<LogItem> TrafficList
        {
            get { return _TrafficList; }
            set
            {
                _TrafficList = value;                
                OnPropertyChanged();
            }
        }

        public void AddToTrafficList(LogItem item)
        {
            while (TrafficList.Count > 16) // Max. 20 Einträge
            {
                TrafficList.RemoveAt(0);
            }

            TrafficList.Add(item);
        }

        #region xxx
        internal static SolidColorBrush GetColorFromTelegram(MelBoxGsm.GsmEventArgs Telegram)
        {           
            switch (Telegram.Type)
            {
                case MelBoxGsm.GsmEventArgs.Telegram.GsmError:
                    return Brushes.Red;
                case MelBoxGsm.GsmEventArgs.Telegram.GsmRec:
                    return Brushes.Green;                    
                case MelBoxGsm.GsmEventArgs.Telegram.GsmSent:
                    return Brushes.Blue;                    
                case MelBoxGsm.GsmEventArgs.Telegram.GsmSystem:
                    return Brushes.DarkGray;                    
                default:
                    return Brushes.Black;                    
            }
        }
        #endregion
    }

    public class LogItem
    {
        public string Message { get; set; }
        public Brush MessageColor { get; set; }
    }
}