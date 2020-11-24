using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;

namespace MelBoxPipe
{
    public class MelBoxPipe
    {
        //Quelle: https://stackoverflow.com/questions/895445/system-io-exception-pipe-is-broken/895656#895656

        public string PipeNameOut { get; set; } = "Server";
        public string PipeNameIn { get; set; } = "Client";

        public void SendToPipe(string sendString)
        {
            if (!IsPipeAvailable(PipeNameOut))
            {
                OnRaisePipeRecEvent("NamedPipe " + PipeNameOut + " ist nicht verfügbar.");
            }
            else
            {
                using (var pipe = new NamedPipeClientStream(".", PipeNameOut, PipeDirection.Out))
                using (var stream = new StreamWriter(pipe))
                {
                    try
                    {

                        pipe.Connect();
                        if (pipe.IsConnected)
                        {
                            stream.Write(sendString);
                        }

                    }
                    catch (TimeoutException ex_T)
                    {
                        OnRaisePipeRecEvent(ex_T.Message);
                    }
                }
            }
        }

        private bool IsPipeAvailable(string pipeName) => Directory.GetFiles(@"\\.\pipe\").Contains($@"\\.\pipe\{pipeName}"); 

        public void ListenToPipe()
        {
            NamedPipeServerStream s = new NamedPipeServerStream(PipeNameIn, PipeDirection.In);
            Action<NamedPipeServerStream> a = callBack;
            a.BeginInvoke(s, ar => { }, null);
        }

        private void callBack(NamedPipeServerStream pipe)
        {
            while (true)
            {
                try
                {
                    pipe.WaitForConnection();
                    StreamReader sr = new StreamReader(pipe);
                    string rec = sr.ReadToEnd();
                    //Console.WriteLine(rec);
                    OnRaisePipeRecEvent(rec);
                    pipe.Disconnect();
                }
                catch (IOException)
                {
                    pipe.Disconnect();
                }
            }
        }


        public event EventHandler<string> RaisePipeRecEvent;

        protected virtual void OnRaisePipeRecEvent(string e)
        {
            RaisePipeRecEvent?.Invoke(this, e);
        }


    }

}