﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace MelBoxGsm
{
    public partial class Gsm
    {
        #region Fields
        private SerialPort Port;
        const int maxConnectTrys = 5;
        int currentConnectTrys = 0;
        #endregion

        #region Properties
        public string CurrentComPortName { get; set; } = "COM1";

        /// <summary>
        /// Liste der anstehenden AT-Commands zur sequenziellen Abarbeitung
        /// </summary>
        private static List<string> ATCommandQueue { get; set; } = new List<string>();
        #endregion

        public void TryConnectPort()
        {
            //int trys = 0;
            //while (trys < 3 && (Port == null || !Port.IsOpen)) //Wiederholter Verbindungsversuch
            //{
            //    ++trys;
            //    //Öffne COM-PORT
            //    if (Port == null)
            //        OnRaiseGsmSystemEvent(new GsmEventArgs(11061347, string.Format("{0}/3 Verbindungsversuch an Port {1}", trys, CurrentComPortName)));
            //    else
            //    {
            //        OnRaiseGsmSystemEvent(new GsmEventArgs(11011901, string.Format("{0}/3 Öffne Port {1}", trys, Port.PortName)));
            //    }

            //    ConnectPort();

            //    if (Port == null || !Port.IsOpen)
            //        Thread.Sleep(2000);
            //}

            ConnectPort();

            if (Port == null || !Port.IsOpen) //Verbindung ist fehlgeschlagen
            {
                ClosePort();
                System.Threading.Thread.Sleep(5000); //Pause zum lesen der Bildschirmausgabe.
                Environment.Exit(0);
            }

            SetupGsm();
        }

        /// <summary>
        /// Verbindet den COM-Port
        /// </summary>
        private void ConnectPort()
        {
            #region richtigen COM-Port ermitteln
            List<string> AvailableComPorts = System.IO.Ports.SerialPort.GetPortNames().ToList();

            if (AvailableComPorts.Count < 1)
            {
                OnRaiseGsmSystemEvent(new GsmEventArgs(11011512, GsmEventArgs.Telegram.GsmError, "Es sind keine COM-Ports vorhanden"));
                return;
            }

            if (!AvailableComPorts.Contains(CurrentComPortName))
            {
                CurrentComPortName = AvailableComPorts.LastOrDefault();
            }
            #endregion

            #region Wenn Port bereits vebunden ist, trennen
            if (Port != null && Port.IsOpen)
            {
                ClosePort();
            }
            #endregion

            #region Verbinde ComPort

            OnRaiseGsmSystemEvent(new GsmEventArgs(11051108, GsmEventArgs.Telegram.GsmSystem, string.Format("Öffne Port {0}...", CurrentComPortName)));

            SerialPort port = new SerialPort();

            while (port == null || !port.IsOpen)
            {
                currentConnectTrys++;
                try
                {
                    port.PortName = CurrentComPortName;                     //COM1
                    port.BaudRate = 9600;                                   //9600
                    port.DataBits = 8;                                      //8
                    port.StopBits = StopBits.One;                           //1
                    port.Parity = Parity.None;                              //None
                    port.ReadTimeout = 300;                                 //300
                    port.WriteTimeout = 300;                                //300
                    port.Encoding = Encoding.GetEncoding("iso-8859-1");
                    port.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);
                    port.ErrorReceived += new SerialErrorReceivedEventHandler(Port_ErrorReceived);
                    port.Open();
                    port.DtrEnable = true;
                    port.RtsEnable = true;

                    if (currentConnectTrys > maxConnectTrys)
                    {
                        OnRaiseGsmSystemEvent(new GsmEventArgs(11061519, GsmEventArgs.Telegram.GsmError ,"Maximale Anzahl Verbindungsversuche zu " + CurrentComPortName + " überschritten."));
                        ClosePort();
                        Environment.Exit(0);
                        return;
                    }
                    else
                    {
                        OnRaiseGsmSystemEvent(new GsmEventArgs(11061554, GsmEventArgs.Telegram.GsmSystem, "Verbindungsversuch " + currentConnectTrys + " von " + maxConnectTrys));
                    }
                }
                catch (ArgumentException ex_arg)
                {
                    OnRaiseGsmSystemEvent(new GsmEventArgs(11011514, GsmEventArgs.Telegram.GsmError, string.Format("COM-Port {0} konnte nicht verbunden werden. \r\n{1}\r\n{2}", CurrentComPortName, ex_arg.GetType(), ex_arg.Message)));
                    Thread.Sleep(2000);
                }
                catch (UnauthorizedAccessException ex_unaut)
                {
                    OnRaiseGsmSystemEvent(new GsmEventArgs(11011514, GsmEventArgs.Telegram.GsmError, string.Format("Der Zugriff auf COM-Port {0} wurde verweigert. \r\n{1}\r\n{2}", CurrentComPortName, ex_unaut.GetType(), ex_unaut.Message)));
                    Thread.Sleep(2000);
                }
                catch (System.IO.IOException ex_io)
                {
                    OnRaiseGsmSystemEvent(new GsmEventArgs(11011514, GsmEventArgs.Telegram.GsmError, string.Format("Verbindungsversuch {0}/{1}: COM-Port {2} konnte erreicht werden. \r\n{3}\r\n{4}", currentConnectTrys, MaxSendRetrys, CurrentComPortName, ex_io.GetType(), ex_io.Message)));
                    Thread.Sleep(2000);
                }

                if (port == null || !port.IsOpen) Thread.Sleep(2000);

            }
            //currentConnectTrys = 0;

            Port = port;
            #endregion
        }

        //Close Port
        public void ClosePort()
        {
            if (Port == null) return;

            OnRaiseGsmSystemEvent(new GsmEventArgs(11011917, GsmEventArgs.Telegram.GsmSystem, "Port " + Port.PortName + " wird geschlossen.\r\n"));
            try
            {
                Port.Close();
                Port.DataReceived -= new SerialDataReceivedEventHandler(Port_DataReceived);
                Port.Dispose();
                Port = null;
                System.Threading.Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AddAtCommand(string command)
        {
            ATCommandQueue.Add(command); //Stellt die Abarbeitung nacheinander sicher
            SendATCommand();
        }

        private void SendATCommand()
        {
            if (Port == null || !Port.IsOpen)
            {
                ConnectPort();
                Thread.Sleep(2000);
            }

            try
            {
                while (ATCommandQueue.Count > 0) //Abarbeitung nacheinander
                {
                    //Thread.Sleep(200);
                    if (Port != null)
                    {
                        #region Warte mit Timeout auf letzte Antwort von GSM-Modem, bevor erneut gesendet wird
                        int n = 0;
                        while (!PermissionToSend && n < 100)
                        {
                            n++;
                            Console.Write('.');
                            System.Threading.Thread.Sleep(50);
                        }
                        Console.WriteLine();
                        PermissionToSend = false;
                        #endregion

                        string command = ATCommandQueue.FirstOrDefault();
                        Port.Write(command + "\r");
                        ATCommandQueue.Remove(command);
                        OnRaiseGsmSystemEvent(new GsmEventArgs(11051045, GsmEventArgs.Telegram.GsmSent, command));
                        Thread.Sleep(400);
                    }
                }
            }
            catch (System.IO.IOException ex_io)
            {
                OnRaiseGsmSystemEvent(new GsmEventArgs(11021909, GsmEventArgs.Telegram.GsmError, ex_io.Message));
            }
            catch (InvalidOperationException ex_inval)
            {
                OnRaiseGsmSystemEvent(new GsmEventArgs(11061455, GsmEventArgs.Telegram.GsmError, ex_inval.Message));
            }
            catch (UnauthorizedAccessException ex_unaut)
            {
                OnRaiseGsmSystemEvent(new GsmEventArgs(11021910, GsmEventArgs.Telegram.GsmError, ex_unaut.Message));
            }

        }

    }
}
