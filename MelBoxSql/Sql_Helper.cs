﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBoxSql
{
    public partial class MelBoxSql
    {

        #region enums
        /// <summary>
        /// Gibt den Weg des Empfangs / der Sendung an. 
        /// </summary>
        [Flags]
        public enum SendToWay
        {
            Undefined = 0,  //nicht bestimmt / ignorieren       
            Sms = 1,        //weiterleiten per SMS
            Email = 2,      //weiterleiten per Email
            None = 4        //nicht weitersenden  
        }

        /// <summary>
        /// Bit-Codierung, an welchen Wochentagen eine Störung gesperrt sein soll. Feiertage zählen als Sonntage.
        /// Alle Tage = 1 + 2 + 4 + 8 + 16 + 32 + 64 = 127
        /// Nur Werktage = 2 + 4 + 8 + 16 + 32 = 62
        /// Nur am Wochenende: 1 + 64 = 65 
        /// </summary>
        [Flags]
        public enum BlockedDays : byte
        {
            Sunday = 1,
            Monday = 2,
            Tuesday = 4,
            Wendsday = 8,
            Thursday = 16,
            Friday = 32,
            Saturday = 64,            
        }

        /// <summary>
        /// Kategorien für Logging
        /// </summary>
        public enum LogTopic
        {
            Allgemein,
            Start,
            Shutdown,
            Sms,
            Email,
            Sql
        }

        /// <summary>
        /// Priorisierung von Log-EInträgen (ggf später auch Meldungen )
        /// </summary>
        public enum LogPrio
        {
            Unknown,
            Error,
            Warning,
            Info
        }

        #endregion

        #region Fields

        #endregion

        #region Helfer-Methoden

        /// <summary>
        /// Extrahiert die ersten beiden Worte als KeyWord aus einem SMS-Text
        /// </summary>
        /// <param name="MessageContent"></param>
        /// <returns>KeyWords</returns>
        internal static string ExtractKeyWords(string MessageContent)
        {
            char[] split = new char[] { ' ', ',', '-', '.', ':', ';' };
            string[] words = MessageContent.Split(split);

            string KeyWords = words[0].Trim();

            if (words.Length > 1)
            {
                KeyWords += " " + words[1].Trim();
            }

            return KeyWords;
        }

        /// <summary>
        /// Wandelt eine Zeitangabe DateTime in einen SQLite-konformen String
        /// </summary>
        /// <param name="dt">Zeit, die umgewandelt werden soll</param>
        /// <returns></returns>
        private static string SqlTime(DateTime dt)
        {
            return dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Gibt True aus, wenn der übergebene String dem Muster einer Email-Adresse enstricht
        /// text@text.text
        /// </summary>
        /// <param name="mailAddress"></param>
        /// <returns>True = Muster Emailadresse erfüllt.</returns>
        internal static bool IsEmail(string mailAddress)
        {
            System.Text.RegularExpressions.Regex mailIDPattern = new System.Text.RegularExpressions.Regex(@"[\w-]+@([\w-]+\.)+[\w-]+");

            if (!string.IsNullOrEmpty(mailAddress) && mailIDPattern.IsMatch(mailAddress))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Kovertiert einen String mit Zahlen und Zeichen in eine Telefonnumer als Zahl mit führender  
        /// Ländervorwahl z.B. +49 (0) 4201 123 456 oder 0421 123 456 wird zu 49421123456 
        /// </summary>
        /// <param name="str_phone">String, der eine Telefonummer enthält.</param>
        /// <returns>Telefonnumer als Zahl mit führender  
        /// Ländervorwahl (keine führende 00). Bei ungültigem str_phone Rückgabewert 0.</returns>
        public static ulong ConvertStringToPhonenumber(string str_phone)
        {
            // Entferne (0) aus +49 (0) 421...
            str_phone = str_phone.Replace("(0)", string.Empty);

            // Entferne alles ausser Zahlen
            System.Text.RegularExpressions.Regex regexObj = new System.Text.RegularExpressions.Regex(@"[^\d]");
            str_phone = regexObj.Replace(str_phone, "");

            // Wenn zu wenige Zeichen übrigbleiben gebe 0 zurück.
            if (str_phone.Length < 2) return 0;

            // Wenn am Anfang 0 steht, aber nicht 00 ersetze führende 0 durch 49
            string firstTwoDigits = str_phone.Substring(0, 2);

            if (firstTwoDigits != "00" && firstTwoDigits[0] == '0')
            {
                str_phone = "49" + str_phone.Substring(1, str_phone.Length - 1);
            }

            ulong number = ulong.Parse(str_phone);

            if (number > 0)
            {
                return number;
            }
            else
            {
                return 0;
            }
        }


        // Aus VB konvertiert
        private static DateTime DateOsterSonntag(DateTime pDate)
        {
            int viJahr, viMonat, viTag;
            int viC, viG, viH, viI, viJ, viL;

            viJahr = pDate.Year;
            viG = viJahr % 19;
            viC = viJahr / 100;
            viH = (viC - viC / 4 - (8 * viC + 13) / 25 + 19 * viG + 15) % 30;
            viI = viH - viH / 28 * (1 - 29 / (viH + 1) * (21 - viG) / 11);
            viJ = (viJahr + viJahr / 4 + viI + 2 - viC + viC / 4) % 7;
            viL = viI - viJ;
            viMonat = 3 + (viL + 40) / 44;
            viTag = viL + 28 - 31 * (viMonat / 4);

            return new DateTime(viJahr, viMonat, viTag);
        }

        // Aus VB konvertiert
        private static List<DateTime> Feiertage(DateTime pDate)
        {
            int viJahr = pDate.Year;
            DateTime vdOstern = DateOsterSonntag(pDate);
            List<DateTime> feiertage = new List<DateTime>
            {
                new DateTime(viJahr, 1, 1),    // Neujahr
                new DateTime(viJahr, 5, 1),    // Erster Mai
                vdOstern.AddDays(-2),          // Karfreitag
                vdOstern.AddDays(1),           // Ostermontag
                vdOstern.AddDays(39),          // Himmelfahrt
                vdOstern.AddDays(50),          // Pfingstmontag
                new DateTime(viJahr, 10, 3),   // TagderDeutschenEinheit
                new DateTime(viJahr, 10, 31),  // Reformationstag
                new DateTime(viJahr, 12, 24),  // Heiligabend
                new DateTime(viJahr, 12, 25),  // Weihnachten 1
                new DateTime(viJahr, 12, 26),  // Weihnachten 2
                new DateTime(viJahr, 12, DateTime.DaysInMonth(viJahr, 12)) // Silvester
            };

            return feiertage;
        }

        public static bool IsHolyday(DateTime date)
        {
            return Feiertage(date).Contains(date);
        }


        #endregion



    }
}

