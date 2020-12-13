
using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using MelBoxSql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBoxWeb
{
    public class MelBoxWebServer
    {
        static void Main()
        {
            //Starten

            var _ = new MelBoxWebServer();
            Console.WriteLine("Starte WebServer.");
        }

        private static bool RunWebServer = true; //Schalter zum Ausschalten des Webservers

        public MelBoxWebServer()
        {
            RunWebServer = true;

            using (var server = new RestServer())
            {
                server.Port = "4444";
                server.LogToConsole(Grapevine.Interfaces.Shared.LogLevel.Warn).Start();

                while (RunWebServer)
                {
                    //nichts unternehmen 
                }
                //server.LogToConsole().Stop();
                server.Stop();
            }
        }
        public static void StopWebServer()
        {
            RunWebServer = false;
        }

    }

    [RestResource]
    public class MelBoxResource
    {
        readonly MelBoxSql.MelBoxSql Sql = new MelBoxSql.MelBoxSql();

        private static string HtmlHead(string htmlTitle)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<html>\n");
            builder.Append("<head>\n");
            builder.Append("<link rel='shortcut icon' href='https://www.kreutztraeger-kaeltetechnik.de/wp-content/uploads/2016/12/favicon.ico'>\n");
            builder.Append("<title>");
            builder.Append("MelBox2 - " + htmlTitle);
            builder.Append("</title>\n");
            builder.Append("<meta name='viewport' content='width=device-width, initial-scale=1'>\n");
            builder.Append("<link rel='stylesheet' href='https://www.w3schools.com/w3css/4/w3.css'>\n");
            builder.Append("<link rel='stylesheet' href='https://fonts.googleapis.com/icon?family=Material+Icons+Outlined'>\n");
            builder.Append("</head>\n");
            builder.Append("<body>\n");
            builder.Append("<p><span class='w3-display-topright'>" + DateTime.Now + "</span></p>\n");
            builder.Append("<div class='w3-bar w3-border'>\n");
            builder.Append("<a href='.\\' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>menu</i></a>\n");
            builder.Append("<a href='.\\in' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>drafts</i></a>\n");
            builder.Append("<a href='.\\out' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>redo</i></a>\n");
            builder.Append("<a href='.\\overdue' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>alarm</i></a>\n");
            builder.Append("<a href='.\\blocked' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>alarm_off</i></a>\n");
            builder.Append("<a href='.\\shift' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined' style='color:lightgrey'>event</i></a>\n");
            builder.Append("<a href='.\\account' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined' style='color:lightgrey'>contact_page</i></a>\n");
            builder.Append("</div>\n");
            builder.Append("<center>\n");
            builder.Append("<div class='w3-container w3-cyan'>\n");
            builder.Append("<h1>MelBox2 - " + htmlTitle + "</h1>\n</div>\n\n");

            return builder.ToString();
        }


        private static string ToHTML_Table(DataTable dt)
        {
            //Quelle: https://stackoverflow.com/questions/19682996/datatable-to-html-table

            StringBuilder builder = new StringBuilder();

            builder.Append(HtmlHead(dt.TableName));
            builder.Append("</div><div class='w3-row'>\n");
            builder.Append("<table class='w3-table-all w3-hoverable w3-cell'>\n");
            builder.Append("<tr class='w3-teak'>\n");
            foreach (DataColumn c in dt.Columns)
            {
                builder.Append("<th>");
                builder.Append(c.ColumnName);
                builder.Append("</th>");
            }
            builder.Append("\n</tr>\n");
            foreach (DataRow r in dt.Rows)
            {
                builder.Append("<tr>\n");
                foreach (DataColumn c in dt.Columns)
                {
                    builder.Append("<td>");
                    builder.Append(r[c.ColumnName]);
                    builder.Append("</td>");
                }
                builder.Append("\n</tr>\n");
            }
            builder.Append("</table>");
            builder.Append("</center>");
            builder.Append("</body>");
            builder.Append("</html>");

            return builder.ToString();
        }

        //BAUSTELEL: Fehlt
        //private static string ToHTML_ShiftTable(DataTable dt)
        //{

        //}

        //BAUSTELLE : Bitzuordnung zu Wochentagen passt noch nicht!
        private static string ToHTML_BlockedTable(DataTable dt)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(HtmlHead(dt.TableName));
            builder.Append("</div><div class='w3-row'>\n");
            builder.Append("<table class='w3-table-all w3-hoverable w3-cell'>\n");
            builder.Append("<tr class='w3-teak'>\n");
            foreach (DataColumn c in dt.Columns)
            {
                builder.Append("<th>");
                builder.Append(c.ColumnName);
                builder.Append("</th>");
            }
            builder.Append("\n</tr>\n");
            foreach (DataRow r in dt.Rows)
            {
                builder.Append("<tr>\n");
                foreach (DataColumn c in dt.Columns)
                {
                    builder.Append("<td>");

                    switch (c.ColumnName)
                    {
                        case "So":
                        case "Mo":
                        case "Di":
                        case "Mi":
                        case "Do":
                        case "Fr":
                        case "Sa":
                            string check = string.Empty;
                            if (r[c.ColumnName].ToString() == "1")                            
                                check = "checked='checked' ";
                            
                            builder.Append("<input class='w3-check' type='checkbox' " + check + ">");
                            break;
                        default:
                            builder.Append(r[c.ColumnName]);
                            break;
                    }

                    builder.Append("</td>\n");
                }
                builder.Append("\n</tr>\n");
            }
            builder.Append("</table>");
            builder.Append("</center>");
            builder.Append("</body>");
            builder.Append("</html>");

            return builder.ToString();
        }


        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/in")]
        public IHttpContext ShowMelBoxIn(IHttpContext context)
        {
            //Tabelle der letzten eingegangenen Nachrichten abrufen, formatieren und wegsenden
            var word = ToHTML_Table(Sql.GetRecMsgView());
            context.Response.SendResponse(word);
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/out")]
        public IHttpContext ShowMelBoxOut(IHttpContext context)
        {
            //Tabelle der letzten ausgegangenen Nachrichten abrufen, formatieren und wegsenden
            var word = ToHTML_Table(Sql.GetSentMsgView());
            context.Response.SendResponse(word);
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/overdue")]
        public IHttpContext ShowMelBoxOverdue(IHttpContext context)
        {
            //Tabelle der letzten ausgegangenen Nachrichten abrufen, formatieren und wegsenden
            var word = ToHTML_Table(Sql.GetOverdueMsgView());
            context.Response.SendResponse(word);
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/blocked")]
        public IHttpContext ShowMelBoxBlocked(IHttpContext context)
        {
            //Tabelle der letzten ausgegangenen Nachrichten abrufen, formatieren und wegsenden
            var word = ToHTML_BlockedTable(Sql.GetBlockedMsgView());
            context.Response.SendResponse(word);
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/repeat")]
        public IHttpContext RepeatMe(IHttpContext context)
        {
            //http://localhost:1234/repeat?word=parrot
            var word = context.Request.QueryString["word"] ?? "what?";
            context.Response.SendResponse(word);
            return context;
        }

        [RestRoute]
        public IHttpContext HelloWorld(IHttpContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append( HtmlHead("Meldesystem") );
            builder.Append("<h2>St&ouml;rmeldesystem f&uuml;r Kreutztr&auml;ger K&auml;ltetechnik</h2>\n");
            builder.Append("<table class='w3-table-all w3-cell'>\n");
            builder.Append("<tr><td><i class='w3-xxlarge material-icons-outlined'>menu</i></td><td><a href='.\\' class='w3-button w3-block w3-teal'>&Uuml;bersicht</a> </td></tr>\n");
            builder.Append("<tr><td><i class='w3-xxlarge material-icons-outlined'>drafts</i></td><td><a href='.\\in' class='w3-button w3-block w3-teal'>Empfangene Nachrichten</a></td></tr>\n");
            builder.Append("<tr><td><i class='w3-xxlarge material-icons-outlined'>redo</i></td><td><a href='.\\out' class='w3-button w3-block w3-teal'>Gesendete Nachrichten</a></td></tr>\n");
            builder.Append("<tr><td><i class='w3-xxlarge material-icons-outlined'>alarm</i></td><td><a href='.\\overdue' class='w3-button w3-block w3-teal'>Fehlende 'OK'-Meldungen</a></td></tr>\n");
            builder.Append("<tr><td><i class='w3-xxlarge material-icons-outlined'>alarm_off</i></td><td><a href='.\\blocked' class='w3-button w3-block w3-teal'>Gesperrte Meldungen</a></td></tr>\n");
            builder.Append("<tr><td><i class='w3-xxlarge material-icons-outlined'>event</i></td><td><a href='.\\shift' class='w3-button w3-block w3-teal'>Bereitschaftsplaner</a></td></tr>\n");
            builder.Append("<tr><td><i class='w3-xxlarge material-icons-outlined'>contact_page</i></td><td><a href='.\\account' class='w3-button w3-block w3-teal'>Stammdaten</a></td></tr>\n");
            builder.Append("</center>\n</body>\n</html>");

            context.Response.SendResponse(builder.ToString());
            return context;
        }
    }


}
