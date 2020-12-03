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
        /// Entfernt eine Nachricht aus den blockierten Einträgen
        /// </summary>
        /// <param name="msgId">ID des Nachrichtentextes aus Tabelle MessageContent</param>
        public void DeleteBlockedMessage(int msgId)
        {

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = "DELETE FROM \"BlockedMessages\" WHERE \"Id\" = @msgId";

                    command.Parameters.AddWithValue("@msgId", msgId);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler InsertBlockedMessage()\r\n" + ex.GetType() + "\r\n" + ex.Message);
            }
        }


    }
}
