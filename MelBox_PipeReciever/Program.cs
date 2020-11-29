using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox_PipeReciever
{
    //Testprogramm für Pipes

   
    class Program
    {
        internal static MelBoxPipe.MelBoxPipe PipeIn = new MelBoxPipe.MelBoxPipe();
        internal static MelBoxPipe.MelBoxPipe PipeOut = new MelBoxPipe.MelBoxPipe();

        internal static string PipeNameIn = "ToManager";
        internal static string PipeNameOut = "ToServer";

        static void Main()
        {

            PipeIn.RaisePipeRecEvent += HandlePipeRecEvent;
            PipeIn.ListenToPipe(PipeNameIn);

            
            Console.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable)
                {
                    PipeOut.SendToPipe(PipeNameOut, PipeNameOut + ": " + DateTime.Now.ToShortTimeString());
                    System.Threading.Thread.Sleep(10000);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);


            Console.WriteLine("Press 'Any'-key..");
            Console.ReadKey();
        }

        static void HandlePipeRecEvent(object sender, string e)
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Pipe IN: " + e);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}
