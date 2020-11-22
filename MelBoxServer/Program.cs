using System;

namespace MelBoxServer
{
    partial class Program
    {
        static void Main()
        {
            MelBoxPipe.PipeServer melBoxPipe = new MelBoxPipe.PipeServer();
            melBoxPipe.RaisePipeRecEvent += HandlePipeRecEvent;

            MelBoxPipe.PipeServer.Out = "Test an Pipe Client";

            MelBoxGsm.Gsm gsm = new MelBoxGsm.Gsm();
            gsm.RaiseGsmFatalErrorEvent += HandleGsmFatalErrorEvent;
            gsm.RaiseGsmSystemEvent += HandleGsmSystemEvent;
            gsm.RaiseGsmSentEvent += HandleGsmSentEvent;
            gsm.RaiseGsmRecEvent += HandleGsmRecEvent;
            gsm.RaiseSmsStatusreportEvent += HandleSmsStatusReportEvent;
            gsm.RaiseSmsRecievedEvent += HandleSmsRecievedEvent;
            gsm.RaiseSmsSentEvent += HandleSmsSentEvent;

            gsm.TryConnectPort(); //TEST

            string cmdLine = "AT";
            gsm.AddAtCommand(cmdLine);


            Console.WriteLine("\r\nAT-Befehl eingeben:");
            while (cmdLine.Length > 0)
            {
                if (cmdLine.StartsWith("send"))
                {
                    string[] s = cmdLine.Split('/');

                    gsm.SmsSend(MelBoxGsm.Gsm.StrToPhone(s[1]), s[2]);
                }
                else
                {                   
                    gsm.AddAtCommand(cmdLine);                   
                }
                cmdLine = Console.ReadLine();
            }

            Console.WriteLine("Beenden mit beliebieger Taste...");
            Console.ReadKey();
            gsm.ClosePort();
        }
    }
}
