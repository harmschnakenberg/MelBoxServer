using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBoxPipe
{
    using System;
    using System.IO;
    using System.IO.Pipes;

    //https://stackoverflow.com/questions/13806153/example-of-named-pipes

    public class PipeServer
    {
        public static string Out { get; set; }

        public event EventHandler<PipeEventArgs> RaisePipeRecEvent;

        protected virtual void OnRaisePipeRecEvent(PipeEventArgs e)
        {
            RaisePipeRecEvent?.Invoke(this, e);
        }

        public PipeServer()
        {
            StartServer();
        }

        //static void Main()
        //{

        //    using (NamedPipeServerStream pipeServer =
        //        new NamedPipeServerStream("testpipe", PipeDirection.Out))
        //    {
        //        Console.WriteLine("NamedPipeServerStream object created.");

        //        // Wait for a client to connect
        //        Console.Write("Waiting for client connection...");
        //        pipeServer.WaitForConnection();

        //        Console.WriteLine("Client connected.");
        //        try
        //        {
        //            // Read user input and send that to the client process.
        //            using (StreamWriter sw = new StreamWriter(pipeServer))
        //            {
        //                sw.AutoFlush = true;
        //                Console.Write("Enter text: ");
        //                sw.WriteLine(Console.ReadLine());
        //            }
        //        }
        //        // Catch the IOException that is raised if the pipe is broken
        //        // or disconnected.
        //        catch (IOException e)
        //        {
        //            Console.WriteLine("ERROR: {0}", e.Message);
        //        }
        //    }
        //}


        void StartServer()
        {
            Task.Run(() =>
            {
                using (NamedPipeServerStream server = new NamedPipeServerStream("MelBoxPipe", PipeDirection.InOut))
                {
                    Console.WriteLine("NamedPipeServerStream erstellt. Warte auf Client...");                    
                    server.WaitForConnection();
                    Console.WriteLine("Client verbunden.");

                    StreamReader reader = new StreamReader(server);
                    StreamWriter writer = new StreamWriter(server);
                    while (true)
                    {
                        string clientRrequest = reader.ReadToEnd();
                        if (clientRrequest.Length > 0)
                        {
                            Console.WriteLine("Empfange " + clientRrequest);
                            PipeEventArgs pipe = new PipeEventArgs
                            {
                                In = clientRrequest
                            };
                            OnRaisePipeRecEvent(pipe);
                        }

                        if (Out.Length > 0)
                        {
                            Console.WriteLine("Sende " + Out);
                            writer.WriteLine(Out);
                            writer.Flush();
                            Out = string.Empty;
                        }
                        
                    }
                }
            });
        }
    }

    public class PipeEventArgs : EventArgs
    {
        public string In { get; set; }
    }
}
