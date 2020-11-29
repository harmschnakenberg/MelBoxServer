using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using static MelBoxGsm.GsmEventArgs;

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
                char[] replace = { '\r', '\n' };

                LogItem log = new LogItem
                {
                    //                    Message = telegram.Message.Replace("\r\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Trim("\r\n"),
                    Message = telegram.Message.Trim(replace),
                    MessageColor = Var.GetColorFromTelegram(telegram)
                };


                Dispatcher.Invoke(new Action(() =>
                    var.TrafficList.Add(log))
                );
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LogItem log = new LogItem
            {
                Message = DateTime.Now.ToString(),
                MessageColor = Brushes.Chocolate
            };

            var.TrafficList.Add(log);
        }


    }


}