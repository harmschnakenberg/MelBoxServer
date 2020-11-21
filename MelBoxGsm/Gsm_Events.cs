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

		/// <summary>
		/// Event 'string gesendet an COM'
		/// </summary>
		public event EventHandler<GsmEventArgs> RaiseGsmSentEvent;

		/// <summary>
		/// Triggert das Event 'string gesendet an COM'
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnRaiseGsmSentEvent(GsmEventArgs e)
		{
			RaiseGsmSentEvent?.Invoke(this, e);
		}

		/// <summary>
		/// Event 'string empfangen von COM'
		/// </summary>
		public event EventHandler<GsmEventArgs> RaiseGsmRecEvent;

		/// <summary>
		/// Triggert das Event 'string empfangen von COM'
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnRaiseGsmRecEvent(GsmEventArgs e)
		{
			RaiseGsmRecEvent?.Invoke(this, e);
		}

        /// <summary>
        /// Event 'string empfangen von COM'
        /// </summary>
        public event EventHandler<GsmEventArgs> RaiseGsmFatalErrorEvent;

        /// <summary>
        /// Triggert das Event 'string empfangen von COM'
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnRaiseGsmFatalErrorEvent(GsmEventArgs e)
        {
            RaiseGsmFatalErrorEvent?.Invoke(this, e);
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
            OnRaiseGsmFatalErrorEvent(new GsmEventArgs(11211443, "Fehler von COM-Port: " + e.EventType));
            ClosePort();
        }

        //Receive data from port
        internal void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (Port == null) return;

            System.Threading.Thread.Sleep(100); //Liest nicht immer den vollständigen Bytesatz 
            string answer = ReadFromPort();

            //if ((answer.Length == 0) || ((!answer.EndsWith("\r\n> ")) && (!answer.EndsWith("\r\nOK\r\n"))))
            if (answer.Contains("ERROR"))
            {
                OnRaiseGsmSystemEvent(new GsmEventArgs(11021909, "Fehlerhaft Empfangen:\n\r" + answer));
            }

            //Send data to whom ever interested
            OnRaiseGsmRecEvent(new GsmEventArgs(11051044, answer));

            //Interpretiere das empfangene auf verwertbare Inhalte
            InterpretGsmRecEvent(null, new GsmEventArgs(111816464, answer));
        }

        /// <summary>
        /// Der eigentliche Lesevorgang von Port
        /// </summary>
        /// <returns></returns>
        private string ReadFromPort()
        {
            try
            {
                int dataLength = Port.BytesToRead;
                byte[] data = new byte[dataLength];
                int nbrDataRead = Port.Read(data, 0, dataLength);
                if (nbrDataRead == 0)
                    return string.Empty;
                return Encoding.ASCII.GetString(data);
            }
            catch (TimeoutException ex_time)
            {
                OnRaiseGsmSystemEvent(new GsmEventArgs(11061406, string.Format("Der Port {0} konnte nicht erreicht werden. Timeout. \r\n{1}\r\n{2}", CurrentComPortName, ex_time.GetType(), ex_time.Message)));
                return string.Empty;
            }
            catch (InvalidOperationException ex_op)
            {
                OnRaiseGsmSystemEvent(new GsmEventArgs(11061406, string.Format("Der Port {0} ist geschlossen \r\n{1}", CurrentComPortName, ex_op.Message)));
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
		public GsmEventArgs(uint id, string message)
		{
			Id = id;
			Message = message;
		}

		public uint Id { get; set; }
		public string Message { get; set; }
	}
}
