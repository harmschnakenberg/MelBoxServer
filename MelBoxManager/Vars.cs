using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MelBoxManager
{


    public class Var : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private BindingList<Tuple<string, string>> _TrafficList = new BindingList<Tuple<string, string>>();

        public BindingList<Tuple<string, string>> TrafficList
        {
            get { return _TrafficList; }
            set
            {
                _TrafficList = value;
                OnPropertyChanged();
            }
        }


    }
    
}