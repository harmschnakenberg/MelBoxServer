using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using MelBoxGsm;

namespace MelBoxManager
{


    public partial class MainWindow : Window
    {

        static readonly Var var = new Var();

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
                GsmEventArgs telegram = Gsm.JSONDeserializeTelegram(e);
                char[] replace = { '\r', '\n' };

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LogItem log = new LogItem
            {
                Message = DateTime.Now.ToString(),
                MessageColor = Brushes.Chocolate
            };

            var.AddToTrafficList(log);
        }


    }


}