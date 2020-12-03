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

        /// <summary>
        /// Neuer Log-Eintrag
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="prio"></param>
        /// <param name="content"></param>
        public void Log(LogTopic topic, LogPrio prio, string content)
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO Log(LogTime, Topic, Prio, Content) VALUES (CURRENT_TIMESTAMP, @topic, @prio, @content)";

                    command.Parameters.AddWithValue("@topic", topic.ToString());
                    command.Parameters.AddWithValue("@prio", (ushort)prio);
                    command.Parameters.AddWithValue("@content", content);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler Log() " + ex.GetType().Name);
            }
        }

        /// <summary>
        /// Neuer Eintrag für Unternehmen
        /// </summary>
        /// <param name="name">Anzeigename des Unternehmens</param>
        /// <param name="address">Standortadresse</param>
        /// <param name="city">PLZ, Ort</param>
        public bool InsertCompany(string name, string address, string city)
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO \"Company\" (\"Name\", \"Address\", \"City\") VALUES (@name, @address, @city );";

                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@address", address);
                    command.Parameters.AddWithValue("@city", city);

                    return 0 != command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                throw new Exception("Sql-Fehler InsertCompany()");
            }
        }

        public bool InsertContact(string name, int companyId, string email, ulong phone, SendToWay sendWay)
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO \"Contact\" (\"Name\", \"CompanyId\", \"Email\", \"Phone\", \"SendWay\" ) VALUES (@name, @companyId, @email, @phone, @sendWay);";

                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@companyId", companyId);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@phone", phone);
                    command.Parameters.AddWithValue("@sendWay", sendWay);

                    return 0 != command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                throw new Exception("Sql-Fehler InsertContact()");
            }
        }

        /// <summary>
        /// Schreibt den Empfang einer neuen Nachricht in die Datenbank.
        /// Gibt die ID der Nachricht in der Empfangsliste aus.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="phone"></param>
        /// <param name="email"></param>
        /// <returns>ID der Empfangenen Nachricht; Wenn nicht erfolgreich 0.</returns>
        public int InsertRecMessage(string message, ulong phone = 0, string email = "")
        {
            int msgId;
            try
            {
                //Absender identifizieren
                int senderId = GetContactId("", phone, email, message);
                //Inhalt identifizieren
                msgId = GetMessageId(message);

                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO \"LogRecieved\" (\"RecieveTime\", \"FromContactId\", \"ContentId\") VALUES " +
                                          "( CURRENT_TIMESTAMP, @fromContactId, @contentId );" +
                                          "SELECT Id FROM \"LogRecieved\" ORDER BY \"RecieveTime\" DESC LIMIT 1";

                    command.Parameters.AddWithValue("@fromContactId", senderId);
                    command.Parameters.AddWithValue("@contentId", msgId);

                    //command.ExecuteNonQuery();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (int.TryParse(reader.GetString(0), out int recId))
                            {
                                Console.WriteLine("Neue SMS mit Empfangs-Id {0} gespeichert.", recId);
                                return msgId;
                            }
                        }
                    }

                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception("InsertMessage()" + ex.GetType() + "\r\n" + ex.Message);
            }
            
        }

        /// <summary>
        /// Erstellt einen neuen Bereitschaftsdienst
        /// </summary>
        /// <param name="contactId">Id des Bereitschaftsnehmers</param>
        /// <param name="startTime">Beginn der Bereitschaft</param>
        /// <param name="endTime">Ende der Bereitschaft</param>
        public void InsertShift(int contactId, DateTime startTime, DateTime endTime)
        {
            try
            {

                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO \"Shifts\" (\"EntryTime\", \"ContactId\", \"StartTime\", \"EndTime\") VALUES " +
                                          "(CURRENT_TIMESTAMP, @contactId, @startTime, @endTime )";

                    command.Parameters.AddWithValue("@contactId", contactId);
                    command.Parameters.AddWithValue("@startTime", SqlTime(startTime));
                    command.Parameters.AddWithValue("@startTime", SqlTime(endTime));

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                throw new Exception("Sql-Fehler InsertShift()");
            }
        }

        /// <summary>
        /// Fügt eine Nachricht der "Blacklist" hinzu, sodass diese bei Empfang nicht weitergeleitet wird.
        /// Ist die Nachricht bereits in der Liste vorhanden, wird kein neuer Eintrag erstellt.
        /// </summary>
        /// <param name="msgId">Id der Nachricht, deren Weiterleitung gesperrt werden soll</param>
        /// <param name="startHour">Tagesstunde - Beginn der Sperre</param>
        /// <param name="endHour">Tagesstunde - Ende der Sperre</param>
        /// <param name="Days">Tage, an denen die Nachricht gesperrt sein soll, wie Wochenuhr; Mo=1, Di=2,...,Werktags=8, Alle Tage=9</param>
        public void InsertBlockedMessage(int msgId, byte blockedDays = 127, int startHour = 17, int endHour = 7)
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    //Nur neuen Eintrag erzeugen, wenn msgId noch nicht vorhanden ist.
                    command.CommandText = "INSERT OR IGNORE INTO \"BlockedMessages\" (\"Id\", \"StartHour\", \"EndHour\", \"Days\" ) VALUES " +
                                          "(@msgId, @startHour, @endHour, @days)";

                    command.Parameters.AddWithValue("@msgId", msgId);
                    command.Parameters.AddWithValue("@startHour", startHour);
                    command.Parameters.AddWithValue("@endHour", endHour);
                    command.Parameters.AddWithValue("@days", blockedDays);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler InsertBlockedMessage()\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// Protokolliert die Weiterleitung einer Nachricht
        /// </summary>
        /// <param name="recMsgId">Id der Nachricht, die weitergeleitet wurde</param>
        /// <param name="sentToId">Id des Kontakts, an den die Nachricht gesendet wurde</param>
        /// <param name="way">Sendeweg (SMS, Email)</param>
        public void InsertLogSent(int recMsgId, int sentToId, SendToWay way)
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO \"LogSent\" (\"LogRecievedId\", \"SentTime\", \"SentToId\", \"SentVia\") " +
                                          "VALUES (@msgId, CURRENT_TIMESTAMP, @sentToId, @sendWay);";

                    command.Parameters.AddWithValue("@msgId", recMsgId);
                    command.Parameters.AddWithValue("@sentToId", sentToId);
                    command.Parameters.AddWithValue("@sendWay", (int)way);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                throw new Exception("Sql-Fehler InsertLogSent()");
            }
        }




    }
}
