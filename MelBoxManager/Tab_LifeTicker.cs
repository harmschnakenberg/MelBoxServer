using MelBoxGsm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MelBoxManager
{
    public partial class MainWindow : Window
    {
        private void HandlePipeRecEvent(object sender, string e)
        {

            if (e.StartsWith("{"))
            {
                GsmEventArgs telegram = Gsm.JSONDeserializeTelegram(e);
                char[] replace = { '\r', '\n' };

                if (var.FilterEvents(telegram))
                {

                    LogItem log = new LogItem
                    {
                        Message = telegram.Message.Replace("\r\n\r\n", "\r\n").Replace("\r\r\n", "\r\n").Trim(replace), //Zeilenumbrüche minimieren
                        MessageColor = Var.GetColorFromTelegram(telegram)
                    };

                    Dispatcher.Invoke(new Action(() => //Dispatcher ist notwendig, um im UI-Thread ändern zu können.
                         var.AddToTrafficList(log)
                    ));
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LogItem log = new LogItem
            {
                Message = DateTime.Now.ToString(),
                MessageColor = System.Windows.Media.Brushes.Chocolate
            };

            var.AddToTrafficList(log);
        }


    }
}
