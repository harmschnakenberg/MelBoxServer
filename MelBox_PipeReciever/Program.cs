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

        internal static string Pipe1 = "ToManager";
        internal static string Pipe2 = "ToServer";

        static void Main(string[] args)
        {

            PipeIn.RaisePipeRecEvent += HandlePipeRecEvent;
            PipeIn.ListenToPipe(Pipe1);

            
            Console.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable)
                {
                    PipeOut.SendToPipe(Pipe2, Pipe2 + ": " + DateTime.Now.ToShortTimeString());
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
