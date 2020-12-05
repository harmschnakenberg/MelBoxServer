using System;


namespace MelBoxServer
{
    partial class Program
    {
        internal static MelBoxPipe.MelBoxPipe PipeIn = new MelBoxPipe.MelBoxPipe();
        internal static MelBoxPipe.MelBoxPipe PipeOut = new MelBoxPipe.MelBoxPipe();

        internal static string PipeNameIn = "ToServer";
        internal static string PipeNameOut = "ToManager";

        internal static MelBoxSql.MelBoxSql Sql = new MelBoxSql.MelBoxSql();

        static void Main()
        {
            PipeIn.RaisePipeRecEvent += HandlePipeRecEvent;
            PipeIn.ListenToPipe(PipeNameIn);

            MelBoxGsm.Gsm gsm = new MelBoxGsm.Gsm();
            //gsm.RaiseGsmFatalErrorEvent += HandleGsmFatalErrorEvent;
            gsm.RaiseGsmSystemEvent += HandleGsmSystemEvent;
            //gsm.RaiseGsmSentEvent += HandleGsmSentEvent;
            //gsm.RaiseGsmRecEvent += HandleGsmRecEvent;
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
