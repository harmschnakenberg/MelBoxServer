using System;
using System.Collections.Generic;

namespace MelBoxGsm
{
    public partial class Gsm
    {
       
        private void SetupGsm()
        {
            RaiseGsmRecEvent += InterpretGsmRecEvent;

            //Textmode
            AddAtCommand("AT+CMGF=1");

            //SendATCommand("AT+CPMS=\"SM\""); //ME, SM, MT
            //SendATCommand("AT+CPMS=\"MT\",\"MT\",\"MT\"");
            AddAtCommand("AT+CPMS=\"SM\",\"SM\",\"SM\"");

            //ID-Nummer der SIM-Karte auslesen 
            AddAtCommand("AT^SCID");

            //Erzwinge, dass bei Fehlerhaftem senden "+CMS ERROR: <err>" ausgegeben wird
            //AddAtCommand("AT^SM20=0,0");

            //SIM-Karte im Mobilfunknetz registriert?
            AddAtCommand("AT+CREG?");

            //Signalqualität
            AddAtCommand("AT+CSQ");

            //Sendeempfangsbestätigungen abonieren
            //Quelle: https://www.codeproject.com/questions/271002/delivery-reports-in-at-commands
            //Quelle: https://www.smssolutions.net/tutorials/gsm/sendsmsat/
            AddAtCommand("AT+CSMP=49,1,0,0");

            AddAtCommand("AT+CNMI=2,1,2,2,1");
            //möglich AT+CNMI=2,1,2,2,1

            //Startet Timer zum wiederholten Abrufen von Nachrichten
            //SetCyclicTimer();
        }

        #region SMS empfangen
        //Wird z.Z. nicht aktiv angefragt
        #endregion

        #region SMS versenden
        /// <summary>
        /// Erstellt und übergibt eine SMS an das GSM-Modem zum senden. Triggert die Sendungsnachverfolgung. 
        /// </summary>
        /// <param name="phone">Telefonnummer an die diese SMS gesendet werden soll (als Zahl mit Ländervorwahl).</param>
        /// <param name="content">Inhalt der SMS</param>
        /// <param name="logSentId">Eindeutige ID für Sendungsnachverfolgung durch Aufrufer. Wird automatisch vergeben, wenn keine Angabe.</param>
        public void SmsSend(ulong phone, string content, int logSentId = 0)
        {
            List<Sms> results = SmsQueue.FindAll(x => x.Phone == phone && x.Content == content);
            if (results.Count == 0)
            {
                //Inhalt vorbereiten
                const string ctrlz = "\u001a";
                content = content.Replace("\r\n", " ");
                if (content.Length > 160) content = content.Substring(0, 160);
                if (logSentId < 1) logSentId = DateTime.Now.Millisecond;

                Sms sms = new Sms
                {
                    LogSentId = logSentId,
                    Phone = phone,
                    Content = content,
                    SendTrys = 1
                };

                //Sendungsnachverfolgung
                SmsQueue.Add(sms);
                SetRetrySendSmsTimer(sms.LogSentId);

                //Senden
                AddAtCommand("AT+CMGS=\"+" + phone + "\"\r");
                AddAtCommand(content + ctrlz);
            }        
        }

        #endregion

        #region SMS löschen
        /// <summary>
        /// Löscht eine SMS aus dem Speicher
        /// </summary>
        /// <param name="smsId">Id der SMS im GSM-Speicher</param>
        private void SmsDelete(int smsId)
        {
            OnRaiseGsmSystemEvent(new GsmEventArgs(11111726, "Die SMS mit der Id " + smsId + " wird gelöscht."));
            
            string cmd = "AT+CMGD=" + smsId;
            // if (ATCommandQueue.Contains(cmd)) return;
            // AddAtCommand(cmd);
            Port.Write(cmd + "\r");
        }

        #endregion


    }
}
