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

        internal static MelBoxSql.MelBoxSql Sql = new MelBoxSql.MelBoxSql();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = var;

            Label_PipeIn.Content = PipeNameIn;
            Label_PipeOut.Content = PipeNameOut;
            
            PipeIn.RaisePipeRecEvent += HandlePipeRecEvent;
            PipeIn.ListenToPipe(PipeNameIn);
        }

        
    }


}