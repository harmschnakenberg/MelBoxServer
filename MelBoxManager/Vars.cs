using MelBoxGsm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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

        #region Funkverkehr CheckBoxen
        private bool _ShowEventGsmRec = true;
        private bool _ShowEventGsmSent = true;
        private bool _ShowEventGsmSystem = true;
        private bool _ShowEventSmsRec = true;
        private bool _ShowEventSmsSent = true;
        private bool _ShowEventSmsStatus = true;

        public bool ShowEventGsmRec
        {
            get { return _ShowEventGsmRec; }
            set
            {
                _ShowEventGsmRec = value;
                OnPropertyChanged();
            }
        }
        public bool ShowEventGsmSent
        {
            get => _ShowEventGsmSent;
            set
            {
                _ShowEventGsmSent = value;
                OnPropertyChanged();
            }
        }
        public bool ShowEventGsmSystem
        {
            get { return _ShowEventGsmSystem; }
            set
            {
                _ShowEventGsmSystem = value; 
                OnPropertyChanged();
            }
        }
        public bool ShowEventSmsRec
        {
            get { return _ShowEventSmsRec; }
            set
            {
                _ShowEventSmsRec = value;
                OnPropertyChanged();
            }
        }
        public bool ShowEventSmsSent
        {
            get { return _ShowEventSmsSent; }
            set
            {
                _ShowEventSmsSent = value;
                OnPropertyChanged();
            }
        }
        public bool ShowEventSmsStatus
        {
            get { return _ShowEventSmsStatus; }
            set
            {
                _ShowEventSmsStatus = value;
                OnPropertyChanged();
            }
        }
        #endregion



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


        private DataTable _TableRec = new DataTable();

        public DataTable TableRec
        {
            get { return _TableRec; }
            set 
            { 
                _TableRec = value;
                OnPropertyChanged();
            }
        }

        private DataTable _TableSent = new DataTable();

        public DataTable TableSent
        {
            get { return _TableSent; }
            set
            {
                _TableSent = value;
                OnPropertyChanged();
            }
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

        internal bool FilterEvents(GsmEventArgs telegram)
        {
            switch (telegram.Type)
            {
                case Telegram.GsmError:
                    return true;
                case Telegram.GsmSystem:
                    return ShowEventGsmSystem;
                case Telegram.GsmRec:
                    return ShowEventGsmRec;
                case Telegram.GsmSent:
                    return ShowEventGsmSent;
                case Telegram.SmsRec:
                    return ShowEventSmsRec;
                case Telegram.SmsStatus:
                    return ShowEventSmsStatus;
                case Telegram.SmsSent:
                    return ShowEventSmsSent;
                default:
                    return true;
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