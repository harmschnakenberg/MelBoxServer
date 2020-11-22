using System;
using System.Collections.Generic;

namespace MelBoxGsm
{
    public partial class Gsm
    {
        #region Properties
        /// <summary>
        /// Liste der zu sendenden Nachrichten. 
        /// Wird genutzt, um Zeitpunkt für Senden und Modem-Antwort zeitlich zu koordinieren
        /// Tuple<Phone, Message>
        /// int = MessageReference <mr>
        /// </summary>
        internal static List<Sms> SmsQueue { get; set; } = new List<Sms>();

        /// <summary>
        /// Merkliste der SMS-Indexe, die zum löschen anstehen
        /// </summary>
        internal static List<int> SmsToDelete = new List<int>();

        public static int MinutesToSendRetry { get; set; } = 2;
        public static int MaxSendRetrys { get; set; } = 5;


        #endregion

        #region Methods
        /// <summary>
        /// Waretet eine Zeit und stößt dann das Lesen des GSM-Speichers an
        /// </summary>
        internal void SetRetrySendSmsTimer()
        {
            System.Timers.Timer aTimer = new System.Timers.Timer(MinutesToSendRetry * 60000); //2 min
            aTimer.Elapsed += (sender, eventArgs) => AddAtCommand("AT+CMGL=\"ALL\"");
            aTimer.AutoReset = false;
            aTimer.Enabled = true;
        }

        private void CheckForResend()
        {
            DebugTracking();
            foreach (Sms sms in SmsQueue)
            {
                if (sms.LastSendTime.AddMinutes(MinutesToSendRetry).CompareTo(DateTime.Now) < 0) //  Kleiner als 0 (null): t1 liegt vor t2.
                {
                    //Zeit für Sendebestätigung überschritten
                    if (sms.SendTrys > MaxSendRetrys)
                    {
                        //Max. Sendeversuche überschritten, Senden verwerfen
                        sms.SendStatus = 254;
                        OnRaiseSmsStatusreportEvent(sms);
                        if (!SmsQueue.Remove(sms))
                        {
                            Console.WriteLine("Die SMS {0} konnte nicht aus der Überwachungsliste entwernt werden.\r\nAn: +{1}\r\n{2}", sms.LogSentId, sms.Phone, sms.Content);
                        }
                        else
                        {
                            Console.WriteLine("Die SMS {0} wurde aus der Sendeungsverfolgung genommen." , sms.LogSentId);
                        }
                    }
                    else
                    {
                        int i = SmsQueue.IndexOf(sms);
                        //Erneut senden
                        SmsQueue[i].SendTrys++; //Sendeversuche hochzählen                                   
                        OnRaiseSmsSentEvent(sms); //Erneutes Senden melden

                        const string ctrlz = "\u001a";
                        AddAtCommand("AT+CMGS=\"+" + sms.Phone + "\"\r"); //Senden
                        AddAtCommand(sms.Content + ctrlz);
                    }
                }
            }
        }
        #endregion

    }

    public class Sms
    {
        #region Properties
        /// <summary>
        /// Von Aufrufer (z.B. DB) vergebene eindeutige ID einer gesendeten Nachricht.
        /// Wird nur "mitgeschleppt", keine Verarbeitung. 
        /// </summary>
        public int LogSentId { get; set; }

        /// <summary>
        /// Index der NAchricht im Modem-Speicher
        /// </summary>
        public byte Index { get; set; }

        /// <summary>
        /// Index der Empfangsbestätigung im Modem-Speicher
        /// </summary>
        public byte TrackingId { get; set; } = 0;

        /// <summary>
        /// Status-Text von GSM-Modem
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Telefonnumer, für Sender / Empfänger (aus Kontext) 
        /// Format mit Ländervorwahl z.B. 49151987654321 (max. 19 Stellen)
        /// </summary>
        public ulong Phone { get; set; }

        /// <summary>
        /// Inhalt der Nachricht
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Status von Sendebestätigung: 
        /// 0-31 erfolfreich von Modem
        /// 32-63 versucht weiter zu senden von Modem
        /// 64-127 sendeversuch abgebrochen von Modem
        /// 254 Abbruch: zu viele Sendeversuche von Programm
        /// 255 Startwert von Programm
        /// </summary>
        public byte SendStatus { get; set; } = 255;

        /// <summary>
        /// Sendeversuche
        /// </summary>
        public int SendTrys { get; set; }

        public DateTime LastSendTime { get; set; } = DateTime.Now;

        #endregion

        #region Methods

        public void SetPhone(string strPhone)
        {
            Phone = Gsm.StrToPhone(strPhone);
        }

        public void SetIndex(string strIndex)
        {
            if( byte.TryParse(strIndex, out byte index))
            Index = index;
        }
        #endregion
    }
}

