using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBoxGsm
{
	public partial class Gsm
	{
        static bool PermissionToSend = true;

        #region Public Basic Gsm Events
        /// <summary>
        /// Event 'System-Ereignis'
        /// </summary>
        public event EventHandler<GsmEventArgs> RaiseGsmSystemEvent;

		/// <summary>
		/// Trigger für das Event 'System-Ereignis'
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnRaiseGsmSystemEvent(GsmEventArgs e)
		{
			RaiseGsmSystemEvent?.Invoke(this, e);
		}

        #endregion

        #region Public Advanced Gsm Events
        /// <summary>
        /// Event SMS empfangen
        /// </summary>
        public event EventHandler<Sms> RaiseSmsRecievedEvent;

        /// <summary>
        /// Trigger für das Event SMS empfangen
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnRaiseSmsRecievedEvent(Sms e)
        {
            RaiseSmsRecievedEvent?.Invoke(this, e);
        }

        /// <summary>
        /// Event SMS erfolgreich (mit Empfangsbestätigung) versendet
        /// </summary>
        public event EventHandler<Sms> RaiseSmsSentEvent;

        /// <summary>
        /// Trigger für das Event SMS erfolgreich versendet
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnRaiseSmsSentEvent(Sms e)
        {
            RaiseSmsSentEvent?.Invoke(this, e);
        }

        /// <summary>
        /// Event SMS erfolgreich (mit Empfangsbestätigung) versendet
        /// </summary>
        public event EventHandler<Sms> RaiseSmsStatusreportEvent;

        /// <summary>
        /// Trigger für das Event SMS erfolgreich versendet
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnRaiseSmsStatusreportEvent(Sms e)
        {
            RaiseSmsStatusreportEvent?.Invoke(this, e);
        }
        #endregion

        #region Internal Events

        internal void Port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Console.WriteLine("Fehler von COM-Port: " + e.EventType);
            OnRaiseGsmSystemEvent(new GsmEventArgs(11211443, GsmEventArgs.Telegram.GsmError, "Fehler von COM-Port: " + e.EventType));
            //ClosePort(); Böse!
        }

        //Receive data from port
        internal void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (Port == null || !Port.IsOpen) return;


            string answer = ReadFromPort();

            //if ((answer.Length == 0) || ((!answer.EndsWith("\r\n> ")) && (!answer.EndsWith("\r\nOK\r\n"))))
            if (answer.Contains("ERROR"))
            {
                OnRaiseGsmSystemEvent(new GsmEventArgs(11021909, GsmEventArgs.Telegram.GsmError, "Fehlerhaft Empfangen:\n\r" + answer));
            }
            else if (answer.Length > 1)
            {
                //Send data to whom ever interested
                OnRaiseGsmSystemEvent(new GsmEventArgs(11051044, GsmEventArgs.Telegram.GsmRec, answer));
            }

            PermissionToSend = true;
        }

        /// <summary>
        /// Der eigentliche Lesevorgang von Port
        /// </summary>
        /// <returns></returns>
        private string ReadFromPort()
        {
            try
            {
                Port.DiscardInBuffer();
                Port.DiscardOutBuffer();
                string answer = string.Empty;
                while (answer.Length < 2)
                {
                    System.Threading.Thread.Sleep(Port.ReadTimeout); //Ist sonst unvollständig
                    answer += Port.ReadExisting();
                }
                return answer; 
            }
            catch (TimeoutException ex_time)
            {
                OnRaiseGsmSystemEvent(new GsmEventArgs(11061406, GsmEventArgs.Telegram.GsmError, string.Format("Der Port {0} konnte nicht erreicht werden. Timeout. \r\n{1}\r\n{2}", CurrentComPortName, ex_time.GetType(), ex_time.Message)));
                return string.Empty;
            }
            catch (InvalidOperationException ex_op)
            {
                OnRaiseGsmSystemEvent(new GsmEventArgs(11061406, GsmEventArgs.Telegram.GsmError, string.Format("Der Port {0} ist geschlossen \r\n{1}", CurrentComPortName, ex_op.Message)));
                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetType().Name + "\r\n" + ex.Message);
            }
        }

        #endregion


    }

    /// <summary>
    /// einfache Ereignisse verursacht durch das Modem 
    /// </summary>
    public class GsmEventArgs : EventArgs
	{
        public enum Telegram
        {
            GsmError,
            GsmSystem,
            GsmRec,
            GsmSent,
            SmsRec,
            SmsStatus,
            SmsSent
        }

        public GsmEventArgs()
        {
            //Benötigt Für JSON Deserialisation
        }

        public GsmEventArgs(uint id, Telegram type, string message)
		{
			Id = id;
            Type = type;
			Message = message;
		}

        public Telegram Type { get; set; }

        public uint Id { get; set; }

		public string Message { get; set; }
	}
}
