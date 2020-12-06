using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MelBoxPipe
{
    public class MelBoxPipe
    {
        //Quelle: https://stackoverflow.com/questions/895445/system-io-exception-pipe-is-broken/895656#895656

        //public string PipeNameOut { get; set; } = "PipeOut";
        //public string PipeNameIn { get; set; } = "PipeIn";

        public void SendToPipe(string pipeName, string sendString)
        {
            //Console.WriteLine("# " + pipeName + " Sendeversuch: " + sendString);
            Task.Run(() =>
            {

                if (IsPipeAvailable(pipeName))
                {
                    using (var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
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
                        catch (Exception ex)
                        {
                            OnRaisePipeRecEvent("PipeSend: " + ex.GetType() + "\r\n" + ex.Message);                            
                        }
                    }
                }
                //else
                //{
                //    OnRaisePipeRecEvent("NamedPipe '" + pipeName + "' ist nicht verfügbar.");
                //}
            });
        }

        private bool IsPipeAvailable(string pipeName) => Directory.GetFiles(@"\\.\pipe\").Contains($@"\\.\pipe\{pipeName}");

        public void ListenToPipe(string pipeName)
        {

            Task.Run(() =>
            {
                try
                {
                    NamedPipeServerStream s = new NamedPipeServerStream(pipeName, PipeDirection.In);
                    Action<NamedPipeServerStream> a = CallBack;
                    a.BeginInvoke(s, ar => { }, null);
                    OnRaisePipeRecEvent("Horche auf '" + pipeName + "'");
                }
                catch (SystemException ex_sys)
                {
                    OnRaisePipeRecEvent("PipeCreateServer: " + ex_sys.Message + "\r\n" + ex_sys.InnerException + "\r\n" + ex_sys.StackTrace);
                }
                catch (Exception ex)
                {
                    OnRaisePipeRecEvent("PipeCreateServer: " + ex.GetType() + "\r\n" + ex.Message);
                }
            });
        }

        private void CallBack(NamedPipeServerStream pipe)
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
                    OnRaisePipeRecEvent("Trenne Pipe.");
                }
                catch (Exception ex)
                {
                    OnRaisePipeRecEvent("PipeRec: " + ex.GetType() + "\r\n" + ex.Message);
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