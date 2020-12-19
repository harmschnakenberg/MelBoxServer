using System;


namespace MelBoxServer
{
    partial class Program
    {
        internal static MelBoxPipe.MelBoxPipe PipeIn = new MelBoxPipe.MelBoxPipe();
        internal static MelBoxPipe.MelBoxPipe PipeOut = new MelBoxPipe.MelBoxPipe();

        internal static string PipeNameIn = "ToServer";
        internal static string PipeNameOut = "ToManager";

        public static MelBoxSql.MelBoxSql Sql = new MelBoxSql.MelBoxSql();
        internal static MelBoxGsm.Gsm Gsm = new MelBoxGsm.Gsm();

        static void Main()
        {
            PipeIn.RaisePipeRecEvent += HandlePipeRecEvent;
            PipeIn.ListenToPipe(PipeNameIn);

            MelBoxWeb.MelBoxWebServer.StartWebServer();

            //#region TEST

            Sql.UpdateShift(1, DateTime.Now, 7, 7, 7);
            Sql.UpdateContact(3, MelBoxSql.MelBoxSql.SendToWay.None);
            //var tuples =  Sql.SafeAndRelayNewMessage("Dies ist eine simulierte Empfangene SMS.", 4916095285304);

            //foreach (var tuple in tuples)
            //{
            //    Console.WriteLine("simuliert Weiterleiten an:\r\n{0}\r\n[{1}] {2}", tuple.Item1, tuple.Item2, tuple.Item3);
            //}

            //Console.WriteLine("Ende Test");
            //Console.ReadKey();
            //#endregion

            //gsm.RaiseGsmFatalErrorEvent += HandleGsmFatalErrorEvent;
            Gsm.RaiseGsmSystemEvent += HandleGsmSystemEvent;
            //gsm.RaiseGsmSentEvent += HandleGsmSentEvent;
            //gsm.RaiseGsmRecEvent += HandleGsmRecEvent;
            Gsm.RaiseSmsStatusreportEvent += HandleSmsStatusReportEvent;
            Gsm.RaiseSmsRecievedEvent += HandleSmsRecievedEvent;
            Gsm.RaiseSmsSentEvent += HandleSmsSentEvent;

            Gsm.TryConnectPort(); //TEST

            string cmdLine = "AT";
            Gsm.AddAtCommand(cmdLine);

            Console.WriteLine("\r\nAT-Befehl eingeben:");
            while (cmdLine.Length > 0)
            {
                if (cmdLine.StartsWith("send"))
                {
                    string[] s = cmdLine.Split('/');

                    Gsm.SmsSend(MelBoxGsm.Gsm.StrToPhone(s[1]), s[2]);
                } else if (cmdLine.StartsWith("sim"))
                {
                    string[] s = cmdLine.Split('/');
                    if (s[1] == "rec")
                    {
                        MelBoxGsm.Sms sms = new MelBoxGsm.Sms
                        {
                            Content = "Dies ist eine simuliert empfangene SMS.",
                            Index = 254,
                            Phone = 4942073559,
                            Status = "REC UNREAD"
                        };

                        HandleSmsRecievedEvent(null, sms);
                    }
                }
                else
                {
                    Gsm.AddAtCommand(cmdLine);
                }
                cmdLine = Console.ReadLine();
            }


            Console.WriteLine("Beenden mit beliebieger Taste...");
            Console.ReadKey();

            MelBoxWeb.MelBoxWebServer.StopWebServer();
            Gsm.ClosePort();
        }
    }
}
