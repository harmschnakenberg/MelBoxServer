using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MelBoxManager
{


    public partial class MainWindow : Window
    {

        static Var var = new Var();

        internal static MelBoxPipe.MelBoxPipe PipeIn = new MelBoxPipe.MelBoxPipe();
        internal static MelBoxPipe.MelBoxPipe PipeOut = new MelBoxPipe.MelBoxPipe();

        internal static string PipeNameIn = "ToManager";
        internal static string PipeNameOut = "ToServer";

        public MainWindow()
        {
            InitializeComponent();
            counterLabel.ItemsSource = var.TrafficList;

            PipeIn.RaisePipeRecEvent += HandlePipeRecEvent;
            PipeIn.ListenToPipe(PipeNameIn);
        }

        private void HandlePipeRecEvent(object sender, string e)
        {

            if (e.StartsWith("{"))
            {
                MelBoxGsm.GsmEventArgs telegram = MelBoxGsm.Gsm.JSONDeserializeTelegram(e);
                string msg = telegram.Message.Replace("\r\n\r\n", "\r\n");

                if (telegram.Type == MelBoxGsm.GsmEventArgs.Telegram.GsmSent)
                {
                    Dispatcher.Invoke(new Action(() =>
                    var.TrafficList.Add(new Tuple<string, string>(msg, "sent"))
                    ));
                }

                if (telegram.Type == MelBoxGsm.GsmEventArgs.Telegram.GsmRec)
                {
                    Dispatcher.Invoke(new Action(() =>
                    var.TrafficList.Add(new Tuple<string, string>(msg, "rec"))
                    ));
                }

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var.TrafficList.Add(new Tuple<string, string>(DateTime.Now.ToString(), "test"));
        }
    }
}