using MelBoxGsm;
using System;

namespace MelBoxServer
{
    partial class Program
	{
        static void HandlePipeRecEvent(object sender, string e)
        {
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Pipe IN: " + e);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

  //      static void HandleGsmFatalErrorEvent(object sender, GsmEventArgs e)
		//{
		//	//Fehlermeldung bei der Kommunikation mit dem COM-Port = Programmabbruch
		//	Console.BackgroundColor = ConsoleColor.DarkRed;
		//	Console.ForegroundColor = ConsoleColor.White;
		//	Console.WriteLine(e.Id + ": " + e.Message);
		//	Console.ForegroundColor = ConsoleColor.Gray;
		//	Console.BackgroundColor = ConsoleColor.Black;
			
		//}

		static void HandleGsmSystemEvent(object sender, GsmEventArgs e)
		{
            switch (e.Type)
            {
                case GsmEventArgs.Telegram.GsmError:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
                case GsmEventArgs.Telegram.GsmSystem:
					Console.ForegroundColor = ConsoleColor.Gray;
					break;
                case GsmEventArgs.Telegram.GsmRec:
					Console.ForegroundColor = ConsoleColor.DarkGreen;
					break;
                case GsmEventArgs.Telegram.GsmSent:
					Console.ForegroundColor = ConsoleColor.DarkMagenta;
					break;
                case GsmEventArgs.Telegram.SmsRec:
					Console.ForegroundColor = ConsoleColor.Cyan;
					break;
                case GsmEventArgs.Telegram.SmsStatus:
					Console.ForegroundColor = ConsoleColor.DarkYellow;
					break;
                case GsmEventArgs.Telegram.SmsSent:
					Console.ForegroundColor = ConsoleColor.DarkBlue;
					break;
                default:
					Console.ForegroundColor = ConsoleColor.White;
					break;
            }

			Console.WriteLine(e.Id + ": " + e.Message);
			Console.ForegroundColor = ConsoleColor.Gray;

			PipeOut.SendToPipe(Pipe2, JSONSerialize(e));

		}

		//static void HandleGsmSentEvent(object sender, GsmEventArgs e)
		//{
		//	//Zeichenfolge, die zum GSM-Modem gesendet wurde
		//	Console.ForegroundColor = ConsoleColor.DarkRed;
		//	Console.WriteLine(e.Id + ": " + e.Message);
		//	Console.ForegroundColor = ConsoleColor.Gray;

		//	PipeOut.SendToPipe(Pipe2, JSONSerialize(e));
		//}

		//static void HandleGsmRecEvent(object sender, GsmEventArgs e)
		//{
		//	//Zeichenfolge die vom GSM-Modem empfangen wurde
		//	Console.ForegroundColor = ConsoleColor.DarkGreen;
		//	Console.WriteLine(e.Id + ": " + e.Message);
		//	Console.ForegroundColor = ConsoleColor.Gray;

		//	PipeOut.SendToPipe(Pipe2, JSONSerialize(e));
		//}

		static void HandleSmsStatusReportEvent(object sender, Sms e)
		{
			//Empfangener Statusreport (z.B. Sendebestätigung)
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(string.Format("SMS {0} konnte {1} zugestellt werden:\r\nAn: +{2}\r\n{3}", e.LogSentId, e.SendStatus < 32 ? "erfolgreich" : "nicht", e.Phone, e.Phone));
			Console.ForegroundColor = ConsoleColor.Gray;

			PipeOut.SendToPipe(Pipe2, JSONSerialize(e));
		}

		static void HandleSmsRecievedEvent(object sender, Sms e)
		{
			//Neue SMS-Nachricht empfangen
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("SMS empfangen:\r\n+" + e.Phone + ": " + e.Content);
			Console.ForegroundColor = ConsoleColor.Gray;

			//TODO: Neue Nachricht in DB eintragen, Weiterleitung triggern
			PipeOut.SendToPipe(Pipe2, JSONSerialize(e));
		}

		static void HandleSmsSentEvent(object sender, Sms e)
		{
			//Neue bzw. erneut SMS-Nachricht versendet
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("SMS versendet:\r\n+" + e.Phone + ": " + e.Content);
			Console.ForegroundColor = ConsoleColor.Gray;

			PipeOut.SendToPipe(Pipe2, JSONSerialize(e));
		}
	}
}
