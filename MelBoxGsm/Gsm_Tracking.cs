using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static int MinutesToSendRetry { get; set; } = 2;
        public static int MaxSendRetrys { get; set; } = 5;


        #endregion

        #region Methods
        /// <summary>
        /// Timer für Sendewiederholungen
        /// </summary>
        internal void SetRetrySendSmsTimer(int uniqueSmsIdentifier)
        {
            System.Timers.Timer aTimer = new System.Timers.Timer(MinutesToSendRetry * 60000); //2 min
            aTimer.Elapsed += (sender, eventArgs) => OnRetrySendSms(uniqueSmsIdentifier);
            aTimer.AutoReset = false;
            aTimer.Enabled = true;
        }

        private void OnRetrySendSms(int uniqueSmsIdentifier)
        {
           List<Sms> retrys = SmsQueue.FindAll(x => x.LogSentId == uniqueSmsIdentifier);

            foreach (Sms sms in retrys)
            {
                if(sms.SendTrys > MaxSendRetrys)
                {
                    //Max. Sendeversuche überschritten, Senden verwerfen
                    sms.SendStatus = 254;
                    OnRaiseSmsStatusreportEvent(sms);
                    SmsQueue.Remove(sms);
                } 
                else
                {
                    //Erneut senden
                    sms.SendTrys++;
                    const string ctrlz = "\u001a";
                    AddAtCommand("AT+CMGS=\"+" + sms.Phone + "\"\r");
                    AddAtCommand(sms.Content + ctrlz);
                    OnRaiseSmsSentEvent(sms);
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
        public byte TrackingId { get; set; }

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

