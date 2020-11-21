using System;

namespace MelBoxServer
{
    partial class Program
    {
        static void Main()
        {
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
                cmdLine = Console.ReadLine();
                gsm.AddAtCommand(cmdLine);
            }

            Console.WriteLine("Beenden mit beliebieger Taste...");
            Console.ReadKey();
            gsm.ClosePort();
        }
    }
}
