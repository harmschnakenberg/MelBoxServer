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
        /// Ändert den Eintrag einer Firma.
        /// Übergebene Parameter < 3 Zeichen werden nicht gegändert.
        /// </summary>
        /// <param name="companyId">Id der Firma</param>
        /// <param name="name">neuer Name der Firma (sonst leer)</param>
        /// <param name="address">neue Anschrift der Firma (sonst leer)</param>
        /// <param name="city">neuer Ort der Firma, ggf. mit PLZ (sonst leer)</param>
        public void UpdateCompany(int companyId, string name = "", string address = "", string city = "")
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    if (name.Length > 3)
                    {
                        command.CommandText = "UPDATE \"Company\" SET \"Name\" = @value WHERE \"Id\" = @companyId;"; ;
                        command.Parameters.AddWithValue("@companyId", companyId);
                        command.Parameters.AddWithValue("@value", name);
                        command.ExecuteNonQuery();
                    }
                    
                    if (address.Length > 3)
                    {
                        command.CommandText = "UPDATE \"Company\" SET \"Address\" = @value WHERE \"Id\" = @companyId;"; ;
                        command.Parameters.AddWithValue("@companyId", companyId);
                        command.Parameters.AddWithValue("@value", address);
                        command.ExecuteNonQuery();
                    }
                    
                    if (city.Length > 3)
                    {
                        command.CommandText = "UPDATE \"Company\" SET \"City\" = @value WHERE \"Id\" = @companyId;"; ;
                        command.Parameters.AddWithValue("@companyId", companyId);
                        command.Parameters.AddWithValue("@value", city);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler UpdateCompany()\r\n" + ex.Message);
            }          
        }


        /// <summary>
        /// Ändert den Eintrag für einen Kontakt.
        /// Übergebene Parameter mit Leerstring bzw. 0 werden nicht gegändert.
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="sendWay">MelBoxSql.SendToWay Sendeweg</param>
        /// <param name="name">Anzeigename</param>
        /// <param name="companyId">Id der Firma</param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="keyWord">Leerstring wird ignoriert</param>
        public void UpdateContact(int contactId, SendToWay sendWay, string name = "", int companyId = 0, string email = "", ulong phone = 0, string keyWord = "")
        {
            //nicht schön: für jede Änderung ein eigener Schreibvorgang
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    if (sendWay != 0)
                    {
                        command.CommandText = "UPDATE \"Contact\" SET \"SendWay\" = @value WHERE \"Id\" = @contactId;"; ;
                        command.Parameters.AddWithValue("@contactId", contactId);
                        command.Parameters.AddWithValue("@value", sendWay);
                        command.ExecuteNonQuery();
                    }

                    if (name.Length > 3)
                    {
                        command.CommandText = "UPDATE \"Contact\" SET \"Name\" = @value WHERE \"Id\" = @contactId;"; ;
                        command.Parameters.AddWithValue("@contactId", contactId);
                        command.Parameters.AddWithValue("@value", name);
                        command.ExecuteNonQuery();
                    }

                    if (companyId > 0)
                    {
                        command.CommandText = "UPDATE \"Contact\" SET \"CompanyId\" = @value WHERE \"Id\" = @contactId;"; ;
                        command.Parameters.AddWithValue("@contactId", contactId);
                        command.Parameters.AddWithValue("@value", companyId);
                        command.ExecuteNonQuery();
                    }

                    if (email.Length > 3 && IsEmail(email))
                    {
                        command.CommandText = "UPDATE \"Contact\" SET \"Email\" = @value WHERE \"Id\" = @contactId;"; ;
                        command.Parameters.AddWithValue("@contactId", contactId);
                        command.Parameters.AddWithValue("@value", email);
                        command.ExecuteNonQuery();
                    }

                    if (phone > 0)
                    {
                        command.CommandText = "UPDATE \"Contact\" SET \"Phone\" = @value WHERE \"Id\" = @contactId;"; ;
                        command.Parameters.AddWithValue("@contactId", contactId);
                        command.Parameters.AddWithValue("@value", phone);
                        command.ExecuteNonQuery();
                    }

                    if (keyWord == null || keyWord.Length > 0) //Leerstring als KeyWord nicht zulässig, aber NULL                
                    {
                        command.CommandText = "UPDATE \"Contact\" SET \"KeyWord\" = @value WHERE \"Id\" = @contactId;"; ;
                        command.Parameters.AddWithValue("@contactId", contactId);
                        command.Parameters.AddWithValue("@value", keyWord ?? "NULL" ); //Wenn keyWord == null Dann string "NULL"

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler UpdateContact()\r\n" + ex.Message);
            }

        }


        // UpdateShift

        // UpdateBlockedMessage

        // UpdateLogSent
        /// <summary>
        /// Ändert Sendestatus im Sendeprotokoll
        /// </summary>
        /// <param name="contendId"></param>
        /// <param name="sendToId"></param>
        /// <param name="confirmStatus"></param>
        public void UpdateLogSent(int logSentId, int sendstatus)
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE \"LogSent\" " +
                                          "SET \"ConfirmStatus\" = @confirmStatus " +
                                          "WHERE \"Id\" = @logSentId ";

                    command.Parameters.AddWithValue("@logSentId", logSentId);                    
                    command.Parameters.AddWithValue("@confirmStatus", sendstatus);

                    command.ExecuteNonQuery();
                    
                }                
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler UpdateLogSent()\r\n" + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        public void UpdateShift(int shiftId, DateTime startDate, int StartHour, int EndHour, int contactId)
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE \"Shifts\" " +
                                          "SET  \"EntryTime\" = CURRENT_TIMESTAMP, "+
                                          "\"StartDate\" = @startDate, " +
                                          "\"StartHour\" = @startHour, " +
                                          "\"EndHour\" = @endHour, " +
                                          "\"ContactId\" = @contactId " +
                                          "WHERE \"Id\" = @shiftId; ";

                    command.Parameters.AddWithValue("@startDate", SqlTime(startDate) );
                    command.Parameters.AddWithValue("@startHour", StartHour);
                    command.Parameters.AddWithValue("@endHour", EndHour);
                    command.Parameters.AddWithValue("@contactId", contactId);
                    command.Parameters.AddWithValue("@shiftId", shiftId);

                    command.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler UpdateShift()\r\n" + ex.GetType() + "\r\n" + ex.Message);
            }            
        }


    }
}
