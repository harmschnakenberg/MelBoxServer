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
        static void Main(string[] args)
        {
            string Pipe1 = "A";
            string Pipe2 = "B";

            MelBoxPipe.MelBoxPipe PipeNameIn = new MelBoxPipe.MelBoxPipe();
            PipeNameIn.RaisePipeRecEvent += HandlePipeRecEvent;
            PipeNameIn.ListenToPipe(Pipe1);

            MelBoxPipe.MelBoxPipe melBoxPipe2 = new MelBoxPipe.MelBoxPipe();
            Console.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable)
                {
                    melBoxPipe2.SendToPipe(Pipe2, Pipe2 + ": " + DateTime.Now.ToShortTimeString());
                    System.Threading.Thread.Sleep(2000);
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
