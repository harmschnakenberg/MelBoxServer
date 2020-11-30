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

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                    SELECT ID FROM MessageContent 
                    WHERE Content = $Content
                    ";

                    command.Parameters.AddWithValue("$Content", content).Size = 160; //Max. 160 Zeichen (oder Bytes?)

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (!int.TryParse(reader.GetString(0), out id))
                            {
                                //Neuen Eintrag erstellen
                                command.CommandText =
                                @"
                                INSERT INTO MessageContent (Content)
                                VALUES ($Content);
                            ";
                                //command.Parameters.AddWithValue("$name", name);
                                command.ExecuteNonQuery();
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

                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT Id " +
                                            "FROM Contact " +
                                            "WHERE  " +
                                            "( length(Name) > 0 AND Name = @name ) " +
                                            "OR ( Phone > 0 AND Phone = @phone ) " +
                                            "OR ( length(Email) > 0 AND Email = @email )" +
                                            "OR ( length(KeyWord) > 0 AND KeyWord = @keyWord ) ";

                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@phone", phone);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@message", message);

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


    }
}
