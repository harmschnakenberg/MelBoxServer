using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MelBoxGsm
{
    public partial class Gsm
    {
        /// <summary>
        /// Wird bei jedem Empfang von Daten durch COM aufgerufen!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InterpretGsmRecEvent(object sender, GsmEventArgs e)
        {
            string input = e.Message;

            if (input.Contains("+CMGS:") || input.Contains("+CMSS:")) //COM-Antwort auf Gesendete SMS
            {
                ParseSmsTrackingIdFromSendResponse(input);
            }
            else
            if (input.Contains("+CDSI:")) //Indikator neuen Statusreport empfangen
            {
                /*
                Meldung einer neu eingegangenen Nachricht von GSM-Modem

                Neuen Statusreport empfangen:
                bei AT+CNMI= [ <mode> ][,  <mt> ][,  <bm> ][,  2 ][,  <bfr> ]
                erwartete Antwort: +CDSI: <mem3>, <index>
                //*/
                ParseRecieveStatusreportIndicator(input);
            }
            else if (input.Contains("+CMTI:")) //Indikator neuee SMS empfangen
            {
                /*
                Meldung einer neu eingegangenen Nachricht von GSM-Modem

                Neue SMS emfangen:
                bei AT+CNMI= [ <mode> ][,  1 ][,  <bm> ][,  <ds> ][,  <bfr> ]
                erwartete Antwort: +CMTI: <mem3>, <index>				
                //*/
                ParseRecieveNewSmsIndicator(input);
            }
            else
            if (input.Contains("+CMGL:")) //Inhalt der gelesenen Nachrichten
            {
                ParseStatusReport(input);
                ParseRecMessages(input);
            }
        }

        /// <summary>
        /// Antwort auf CMGS=[...]
        /// Nachricht wurde gesendet. Liest internen Index für Statusreport aus
        /// </summary>
        /// <param name="input"></param>
        private void ParseSmsTrackingIdFromSendResponse(string input)
        {
            if (input == null) return;
            /* z.B.
            +CMGS: 67

            OK
            */
            Regex r = new Regex(@"\+CMGS: (\d+)");
            Match m = r.Match(input);

            while (m.Success)
            {
                if (byte.TryParse(m.Groups[1].Value, out byte trackingId))
                {
                    Sms firstNew = SmsQueue.FindAll(x => x.TrackingId == 0).FirstOrDefault();
                    if (firstNew == null) continue;
                    firstNew.TrackingId = trackingId;
                    OnRaiseSmsSentEvent(firstNew);
                }
                m = m.NextMatch();
            }
        }

        /// <summary>
        /// Antwort auf AT+CNMI=2,1,2,2,1
        /// Ereignis: Neue SMS/StatusReport empfangen
        /// </summary>
        /// <param name="input"></param>
        private void ParseRecieveStatusreportIndicator(string input)
        {
            string pattern = "\\+CDSI: \"\\w+\",(\\d+)";
            string strResp2 = System.Text.RegularExpressions.Regex.Match(input, pattern).Groups[1].Value;
            if (strResp2 == null || strResp2.Length < 1)
                return;
            int.TryParse(strResp2, out int IdNewMsg);

            OnRaiseGsmSystemEvent(new GsmEventArgs(11181057, "Stausreport empfangen mit Index " + IdNewMsg));
            //Lese neue Nachricht(en) [CMGL, damit in Antwort Index mitgelesen wird]
            AddAtCommand("AT+CMGL=\"REC UNREAD\"");
        }

        private void ParseRecieveNewSmsIndicator(string input)
        {
            string pattern = "\\+CMTI: \"\\w+\",(\\d+)";
            string strResp2 = System.Text.RegularExpressions.Regex.Match(input, pattern).Groups[1].Value;
            if (strResp2 == null || strResp2.Length < 1)
                return;
            int.TryParse(strResp2, out int IdNewMsg);

            OnRaiseGsmSystemEvent(new GsmEventArgs(11181057, "Neue SMS empfangen mit Index " + IdNewMsg));
            //Lese neue Nachricht(en) [CMGL, damit in Antwort Index mitgelesen wird]
            AddAtCommand("AT+CMGL=\"REC UNREAD\"");
        }

        /// <summary>
        /// Lese Statusreport-SMS, lösche bei erfolgreichem Senden SMS aus Sendungsverfolgung
        /// </summary>
        /// <param name="input"></param>
        private void ParseStatusReport(string input)
        {
            if (input == null) return;
            try
            {
                //+CMGL: < index > ,  < stat > ,  < fo > ,  < mr > , [ < ra > ], [ < tora > ],  < scts > ,  < dt > ,  < st >
                //[... ]
                //OK
                //<st> 0-31 erfolgreich versandt; 32-63 versucht weiter zu senden: 64-127 Sendeversuch abgebrochen

                //z.B.: +CMGL: 1,"REC READ",6,34,,,"20/11/06,16:08:45+04","20/11/06,16:08:50+04",0
                //Regex r = new Regex(@"\+CMGL: (\d+),""(.+)"",(\d+),(\d+),,,""(.+)"",""(.+)"",(\d+)\r\n");
                Regex r = new Regex(@"\+CMGL: (\d+),""(.+)"",(\d+),(\d+),,,""(.+)"",""(.+)"",(\d+)");
                Match m = r.Match(input);

                while (m.Success)
                {
                    byte.TryParse(m.Groups[1].Value, out byte index);
                    string status = m.Groups[2].Value;
                    byte trackingId = byte.Parse(m.Groups[4].Value);
                    byte trackingStatus = byte.Parse(m.Groups[7].Value);

                    if (status != "REC UNREAD" && status != "REC READ") continue; //nur empfangene Nachrichten

                    Sms confiredSms = SmsQueue.FindAll(x => x.TrackingId == trackingId).FirstOrDefault();
                    if (confiredSms != null)
                    {
                        confiredSms.SendStatus = trackingStatus;
                        OnRaiseSmsStatusreportEvent(confiredSms);

                        if (confiredSms.SendStatus < 32) //SMS erfolgreich versendet
                        {
                            SmsQueue.Remove(confiredSms);
                        }
                    }
                        SmsDelete(index); //Diesen Statusreport löschen
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// BAUSTELLE
        /// </summary>
        /// <param name="input"></param>
        private void ParseRecMessages(string input)
        {
            if (input == null) return;
            try
            {
                #region comment 
                /*
                Output if text mode ( AT+CMGF =1) and command successful: 
                For SMS-DELIVER                                                          
                +CMGR: <stat> ,  <oa> , [ <alpha> ],  <scts> [,  <tooa> ,  <fo> ,  <pid> ,  <dcs> ,  <sca> ,  <tosca> , 
                <length> ]
                <data>
                [... ]
                OK
                For SMS-SUBMIT 
                +CMGR: <stat> ,  <da> , [ <alpha> ][,  <toda> ,  <fo> ,  <pid> ,  <dcs> , [ <vp> ],  <sca> ,  <tosca> ,  <length> ]
                <data>
                [... ]
                OK
                For SMS-STATUS-REPORT 
                +CMGR: <stat> ,  <fo> ,  <mr> , [ <ra> ], [ <tora> ],  <scts> ,  <dt> ,  <st>
                <data>
                [... ]
                OK

                <stat> : Status {string} 
                <oa> Originating Address (Quellle) {string}
                <da> Destination Address (Ziel) {string}
                <fo> First Octet Status-Byte (keine Erklärung gefunden) {byte}
                <mr> Message Reference (= Bezugs-Id in Statusreport) {byte}
                <svcts> ServiceCenterTimeStamp {string}
                <toda> / <tora> / <tooa> Byte Typ der Ziel-/Empfängeradresse 145 = "+49...", sonst 129 {byte}

                +CMGL: 9,"REC READ","+4917681371522",,"20/11/08,13:47:10+04"
                Ein Test 08.11.2020 13:46 PS sms38.de
                //*/
                #endregion

                Regex r = new Regex(@"\+CMGL: (\d+),""(.+)"",""(.+)"",(.*),""(.+)""\r\n(.+)");
                Match m = r.Match(input);

                while (m.Success)
                {
                    Sms sms = new Sms();
                    sms.SetIndex(m.Groups[1].Value);
                    sms.Status = m.Groups[2].Value;
                    sms.Phone = StrToPhone(m.Groups[3].Value);
                    sms.Content = m.Groups[6].Value;

                    if (sms.Status == "REC UNREAD" || sms.Status == "REC READ")
                    {
                        OnRaiseSmsRecievedEvent(sms);
                        SmsDelete(sms.Index);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}