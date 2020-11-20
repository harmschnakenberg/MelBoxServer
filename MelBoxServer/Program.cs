using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBoxServer
{
    partial class Program
    {
        static void Main()
        {
            MelBoxGsm.Gsm gsm = new MelBoxGsm.Gsm();
            gsm.RaiseGsmSystemEvent += HandleGsmSystemEvent;
            gsm.RaiseGsmSentEvent += HandleGsmSentEvent;
            gsm.RaiseGsmRecEvent += HandleGsmRecEvent;
            gsm.RaiseSmsStatusreportEvent += HandleSmsStatusReportEvent;
            gsm.RaiseSmsRecievedEvent += HandleSmsRecievedEvent;
            gsm.RaiseSmsSentEvent += HandleSmsSentEvent;

          
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
