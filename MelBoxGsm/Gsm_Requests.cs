﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MelBoxGsm
{
    public partial class Gsm
    {
       
        private void SetupGsm()
        {

            //Textmode
            AddAtCommand("AT+CMGF=1");

            //SendATCommand("AT+CPMS=\"SM\""); //ME, SM, MT
            //SendATCommand("AT+CPMS=\"MT\",\"MT\",\"MT\"");
            AddAtCommand("AT+CPMS=\"SM\",\"SM\",\"SM\"");

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

        #endregion

        #region SMS versenden
        public void SmsSend(int logSentId, ulong phone, string content)
        {
            List<Sms> results = SmsQueue.FindAll(x => x.LogSentId == logSentId);
            if (results.Count == 0)
            {
                //Inhalt 
                const string ctrlz = "\u001a";
                content = content.Replace("\r\n", " ");
                if (content.Length > 160) content = content.Substring(0, 160);

                //Tracking
                Sms sms = new Sms
                {
                    LogSentId = logSentId,
                    Phone = phone,
                    Content = content,
                    SendTrys = 1
                };

                SmsQueue.Add(sms);
                SetRetrySendSmsTimer(logSentId);

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

            AddAtCommand("AT+CMGD=" + smsId);
        }

        #endregion


    }
}