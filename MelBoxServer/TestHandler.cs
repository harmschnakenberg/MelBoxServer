using MelBoxGsm;
using System;

namespace MelBoxServer
{
    partial class Program
	{
		static void HandleGsmFatalErrorEvent(object sender, GsmEventArgs e)
		{
			Console.BackgroundColor = ConsoleColor.DarkRed;
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(e.Id + ": " + e.Message);
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.BackgroundColor = ConsoleColor.Black;
			
		}

		static void HandleGsmSystemEvent(object sender, GsmEventArgs e)
		{
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine(e.Id + ": " + e.Message);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		static void HandleGsmSentEvent(object sender, GsmEventArgs e)
		{
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine(e.Id + ": " + e.Message);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		static void HandleGsmRecEvent(object sender, GsmEventArgs e)
		{
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine(e.Id + ": " + e.Message);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		static void HandleSmsStatusReportEvent(object sender, Sms e)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(string.Format("SMS {0} konnte {1} zugestellt werden:\r\nAn: +{2}\r\n{3}", e.LogSentId, e.SendStatus < 32 ? "erfolgreich" : "nicht", e.Phone, e.Phone));
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		static void HandleSmsRecievedEvent(object sender, Sms e)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("SMS empfangen:\r\n+" + e.Phone + ": " + e.Content);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		static void HandleSmsSentEvent(object sender, Sms e)
		{
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("SMS versendet:\r\n+" + e.Phone + ": " + e.Content);
			Console.ForegroundColor = ConsoleColor.Gray;
		}
	}
}
