using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace MelBoxSql
{
    public partial class MelBoxSql
    {


        private readonly string DataSource = "Data Source=" + DbPath;

        //Pfad zur Datenbank-Datei; Standartwert: Unterordner "DB" dieser exe
        public static string DbPath { get; set; } = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DB", "MelBox2.db");

        #region Methods

        public MelBoxSql()
        {
            //Datenbak prüfen / erstellen
            if (!System.IO.File.Exists(DbPath))
            {
                CreateNewDataBase();
            }
        }

        /// <summary>
        /// Erzeugt eine neue Datenbankdatei, erzeugt darin Tabellen, Füllt diverse Tabellen mit Defaultwerten.
        /// </summary>
        private void CreateNewDataBase()
        {
            //Erstelle Datenbank-Datei und öffne einmal 
            Directory.CreateDirectory(Path.GetDirectoryName(DbPath));
            FileStream stream = File.Create(DbPath);
            stream.Close();

            //Erzeuge Tabellen in neuer Datenbank-Datei
            //Zeiten im Format TEXT (Lesbarkeit Rohdaten)
            using (var connection = new SqliteConnection(DataSource))
            {
                SQLitePCL.Batteries.Init();
                //SQLitePCL.raw.SetProvider(new  );

                connection.Open();

                List<String> TableCreateQueries = new List<string>
                    {
                        //Debug Log
                        "CREATE TABLE \"Log\"(\"Id\" INTEGER NOT NULL PRIMARY KEY UNIQUE,\"LogTime\" TEXT NOT NULL, \"Topic\" TEXT , \"Prio\" INTEGER NOT NULL, \"Content\" TEXT);",

                        //Kontakte
                        "CREATE TABLE \"Company\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"Name\" TEXT NOT NULL, \"Address\" TEXT, \"City\" TEXT); ",

                        "CREATE TABLE \"Contact\"(\"Id\" INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE, \"EntryTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, \"Name\" TEXT NOT NULL, " +
                        "\"CompanyId\" INTEGER, \"Email\" TEXT, \"Phone\" INTEGER, \"KeyWord\" TEXT, \"MaxInactiveHours\" INTEGER DEFAULT 0, \"SendWay\" INTEGER DEFAULT 0);",

                        //Nachrichten
                        "CREATE TABLE \"MessageContent\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"Content\" TEXT NOT NULL UNIQUE );",

                        "CREATE TABLE \"LogRecieved\"( \"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"RecieveTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, \"FromContactId\" INTEGER NOT NULL, \"ContentId\" INTEGER NOT NULL);",

                        "CREATE TABLE \"LogSent\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"LogRecievedId\" INTEGER NOT NULL, \"SentTime\" TEXT NOT NULL, \"SentToId\" INTEGER NOT NULL, \"SentVia\" INTEGER NOT NULL, \"ConfirmStatus\" INTEGER NOT NULL DEFAULT -1);" +
                        
                        //Bereitschaft
                        "CREATE TABLE \"Shifts\"( \"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"EntryTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, " +
                        "\"ContactId\" INTEGER NOT NULL, \"StartTime\" TEXT NOT NULL, \"HoursDuration\" INTEGER NOT NULL );",

                        "CREATE TABLE \"BlockedMessages\"( \"Id\" INTEGER NOT NULL UNIQUE, \"EntryTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, \"StartHour\" INTEGER NOT NULL, " +
                        "\"EndHour\" INTEGER NOT NULL, \"Days\" INTEGER NOT NULL);",

                        //Hilfstabelle
                        "CREATE TABLE \"SendWay\" ( \"Way\" TEXT NOT NULL UNIQUE, \"Code\" INTEGER NOT NULL UNIQUE);",
                        "INSERT INTO \"SendWay\" (\"Way\", \"Code\") VALUES ('undefiniert', " + (int)SendToWay.Undefined + ");",
                        "INSERT INTO \"SendWay\" (\"Way\", \"Code\") VALUES ('SMS', " + (int)SendToWay.Sms + ");",
                        "INSERT INTO \"SendWay\" (\"Way\", \"Code\") VALUES ('Email', " + (int)SendToWay.Email + ");",
                        "INSERT INTO \"SendWay\" (\"Way\", \"Code\") VALUES ('Dummy', " + (int)SendToWay.None + ");",

                        //Views
                        "CREATE VIEW \"RecievedMessages\" AS SELECT r.Id As EmpfangNr, RecieveTime AS Empfangen, c.Name AS von, Content AS Inhalt " +
                        "FROM LogRecieved AS r JOIN Contact AS c ON FromContactId = c.Id JOIN MessageContent AS m ON ContentId = m.Id",
                        
                        "CREATE VIEW \"SentMessages\" AS SELECT LogRecievedId As EmpfangNr, Content AS Inhalt, SentTime AS Gesendet, Name AS An, Way AS Medium, ConfirmStatus As Sendestatus " +
                        "FROM LogSent AS ls JOIN Contact AS c ON SentToId =  c.Id JOIN SendWay AS sw ON c.SendWay = sw.Code JOIN LogRecieved AS lr ON lr.Id = ls.LogRecievedId JOIN MessageContent AS mc ON mc.id = lr.FromContactId"
                };

                foreach (string query in TableCreateQueries)
                {

                    var command = connection.CreateCommand();
                    command.CommandText = query;
                    command.ExecuteNonQuery();
                }

                InsertCompany("_UNBEKANNT_", "Musterstraße 123", "12345 Modellstadt");
                InsertCompany("Kreutzträger Kältetechnik GmbH & Co. KG", "Theodor-Barth-Str. 21", "28307 Bremen");

                InsertContact("SMSZentrale", 1, "smszentrale@kreutztraeger.de", 4915142265412, SendToWay.None);
                InsertContact("MelBox2Admin", 1, "harm.schnakenberg@kreutztraeger.de", 0, SendToWay.Sms & SendToWay.Email);
                InsertContact("Bereitschaftshandy", 1, "bereitschaftshandy@kreutztraeger.de", 491728362586, SendToWay.None);
                InsertContact("Kreutzträger Service", 1, "service@kreutztraeger.de", 0, SendToWay.None);
                InsertContact("Henry Kreutzträger", 1, "henry.kreutztraeger@kreutztraeger.de", 491727889419, SendToWay.None);
                InsertContact("Bernd Kreutzträger", 1, "bernd.kreutztraeger@kreutztraeger.de", 491727875067, SendToWay.None);
                InsertContact("Harm privat", 1, "harm.schnakenberg@kreutztraeger.de", 4916095285304, SendToWay.Sms);

                InsertRecMessage("Datenbank neu erstellt.", 0, "smszentrale@kreutztraeger.de");

                InsertLogSent(1, 1, SendToWay.None);

                InsertShift(2, DateTime.Now.Date.AddHours(-7), 10);

                InsertBlockedMessage(1);
            }

        }

        #endregion
    }
}