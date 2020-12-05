using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBoxSql
{
    public partial class MelBoxSql
    {
        public int GetMessageId(string content)
        {
            try
            {
                int id = 0;

                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    //Neuen Eintrag erstellen, wenn er nicht existiert
                    var command1 = connection.CreateCommand();
                    command1.CommandText = @"
                                INSERT OR IGNORE INTO MessageContent (Content)
                                VALUES ($Content);                                 
                                ";

                    command1.Parameters.AddWithValue("$Content", content).Size = 160; //Max. 160 Zeichen (oder Bytes?)
                    command1.ExecuteNonQuery();


                   // var command2 = connection.CreateCommand();
                    command1.CommandText = @"
                    SELECT ID FROM MessageContent 
                    WHERE Content = $Content
                    ";

                    //using (var reader = command2.ExecuteReader())
                    //{
                        //if (!reader.HasRows)
                        //{
                        //    //Neuen Eintrag erstellen
                        //    var command2 = connection.CreateCommand();
                        //    command2.CommandText = @"
                        //        INSERT INTO MessageContent (Content)
                        //        SELECT ($Content);
                        //        WHERE NOT EXISTS(SELECT 1 FROM MessageContent WHERE Content = $Content);
                        //        ";
                        //    //command.Parameters.AddWithValue("$name", name);
                        //    command2.ExecuteNonQuery();
                        //}
                    //}

                    using (var reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (!int.TryParse(reader.GetString(0), out id))
                            {
                                
                                //Neu erstellten Eintrag lesen
                                id = GetMessageId(content);
                            }
                            //Console.WriteLine($"Hello, {name}!");
                        }
                    }
                }

                if (id == 0)
                    //Provisorisch:
                    throw new Exception("GetMessageId() Kontakt konnte nicht zugeordnet werden.");

                return id;
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetMessageId()" + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// Versucht den Kontakt anhand der Telefonnummer, email-Adresse oder dem Beginn eriner Nachricht zu identifizieren
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="email"></param>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        public int GetContactId(string name = "", ulong phone = 0, string email = "", string message = "")
        {
            int id = 0;

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    string keyWords = ExtractKeyWords(message);

                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT Id " +
                                            "FROM Contact " +
                                            "WHERE  " +
                                            "( length(Name) > 0 AND Name = @name ) " +
                                            "OR ( Phone > 0 AND Phone = @phone ) " +
                                            "OR ( length(Email) > 0 AND Email = @email ) " +
                                            "OR ( length(KeyWord) > 0 AND KeyWord = @keyWord ) ";

                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@phone", phone);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@message", message);
                    command.Parameters.AddWithValue("@keyWord", keyWords);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (!int.TryParse(reader.GetString(0), out id))
                            {
                                //Neuen Eintrag erstellen
                                InsertContact(name, 0, email, phone, SendToWay.None);

                                //Neu erstellten Eintrag lesen
                                id = GetContactId(name, phone, email, message);
                            }
                        }
                    }
                }

                return id;
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetContactId()" + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// Listet die Telefonnummern der aktuellen SMS-Empfänger (Bereitschaft) auf.
        /// Wenn für den aktuellen Tag keine Bereitschaft eingerichtet ist, wird das Bereitschaftshandy eingesetzt.
        /// </summary>
        /// <returns>Liste der Telefonnummern derer, die zum aktuellen Zeitpunkt per SMS benachrichtigt werden sollen.</returns>
        public List<ulong> GetCurrentShiftPhoneNumbers()
        {
            const string StandardWatchName = "Bereitschaftshandy"; 

            try
            {
                List<ulong> watch = new List<ulong>();

                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    #region Stelle sicher, dass es jetzt eine eine aktive Schicht gibt.
                    command.CommandText = @"SELECT Id " +
                                            "FROM Shifts " +
                                            "WHERE  " +
                                            "CURRENT_TIMESTAMP BETWEEN StartTime AND EndTime";

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            int contactIdBereitschafshandy = GetContactId(StandardWatchName);
                            if (contactIdBereitschafshandy == 0)
                            {
                                throw new Exception(" GetCurrentShiftPhoneNumbers(): Kein Kontakt '" + StandardWatchName + "' gefunden.");
                            }

                            //Erzeuge eine neue Schicht für heute mit Standardwerten (Bereitschaftshandy)
                            DateTime monday = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);
                            InsertShift(contactIdBereitschafshandy, monday, monday.AddDays(7));
                        }
                      
                    }
                    #endregion

                    #region Lese Telefonnummern der laufenden Schicht aus der Datenbank
                    command.CommandText =   "SELECT \"Phone\" FROM Contact " +
                                            "WHERE \"Phone\" > 0 AND " +
                                            "\"Id\" IN " +
                                            "( SELECT ContactId FROM Shifts WHERE CURRENT_TIMESTAMP BETWEEN StartTime AND EndTime )";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (ulong.TryParse(reader.GetString(0), out ulong phone))
                            {
                                watch.Add(phone);
                            }
                        }
                    }
                    #endregion
                }

                if (watch.Count == 0)
                    throw new Exception(" GetCurrentShiftPhoneNumbers(): Es ist aktuell keine SMS-Bereitschaft definiert.");

                return watch;
            }
            catch (Exception ex)
            {
                throw new Exception(" GetCurrentShiftPhoneNumbers()" + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// Gibt an, ob die Nachricht zur Zeit für Weiterleitung gesperrt ist.
        /// </summary>
        /// <param name="messageId">ID des Nachrichtentextes</param>
        /// <returns></returns>
        public bool IsMessageBlocked(int messageId)
        {
            using (var connection = new SqliteConnection(DataSource))
            {
                DateTime now = DateTime.Now; //Lokale Zeit
                int hourNow = now.Hour;
                byte blockedDays = 0;

                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT \"StartHour\", \"EndHour\", \"Days\" FROM \"BlockedMessages\" WHERE Id = @messageId";

                command.Parameters.AddWithValue("@messageId", messageId);

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows) return false; // Nachricht ist nicht in der Blacklist

                    while (reader.Read())
                    {
                        //Ist die Nachricht zum jetzigen Zeitpunt geblockt?
                        if (!int.TryParse(reader.GetString(0), out int startHour)) return false;
                        if (!int.TryParse(reader.GetString(1), out int endHour)) return false;
                        if (!byte.TryParse(reader.GetString(1), out blockedDays)) return false;

                        if (startHour > hourNow && endHour <= hourNow) return false; //Uhrzeit für Block nicht erreicht
                    }
                }

                DayOfWeek dayOfWeek = now.DayOfWeek;
                if (IsHolyday(now)) dayOfWeek = DayOfWeek.Sunday; //Feiertage sind wie Sonntage

                bool isBlocked = ((byte)dayOfWeek & blockedDays) > 0; // Ist das Bit dayOfWeek im byte blockedDays vorhanden?

                Console.WriteLine("IsMessageBlocked() Geblockte Tage: [byte] " + blockedDays + ", heute: " + (byte)dayOfWeek + ", also " + (isBlocked ? "jetzt geblockt." : "nicht geblockt.") );

                return isBlocked;

            }
        }

    }
}
