
using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using System;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MelBoxWeb
{
    public class MelBoxWebServer
    {
        // Create CancellationTokenSource.
       static readonly CancellationTokenSource source = new CancellationTokenSource();

        public static string Port { get; set; } = "4444";

        static void RunWebServer(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                Console.WriteLine("Web-Server beendet.");
                return;
            }

            using (var server = new RestServer())
            {
                server.Port = Port;
                server.LogToConsole(Grapevine.Interfaces.Shared.LogLevel.Warn).Start();

                while (!token.IsCancellationRequested)
                {
                    //nichts unternehmen 
                }
                //server.LogToConsole().Stop();
                server.Stop();
            }
        }

        public static void StartWebServer()
        {

            // ... Get Token from source.
            var token = source.Token;

            // Run the DoSomething method and pass it the CancellationToken.
            // ... Specify the CancellationToken in Task.Run.
            var task = Task.Run(() => RunWebServer(token), token);
        }

        public static void StopWebServer()
        {
            source.Cancel();
        }

    }

    [RestResource]
    public class MelBoxResource
    {
        

#if DEBUG
        static int LogedInContactId = 2; 
        static string username = "MelBox2Admin";
#else
        static int LogedInContactId = 0;
        static string username = string.Empty;
#endif



        public static string Disabled { 
            get
            {
                return (LogedInContactId == 0) ? "disabled" : string.Empty;
            }
        }

        static readonly MelBoxSql.MelBoxSql Sql = new MelBoxSql.MelBoxSql();

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
            builder.Append("<p><span class='w3-display-topright'>" + username + " "+ DateTime.Now + "</span></p>\n");
            builder.Append("<div class='w3-bar w3-border'>\n");
            builder.Append("<a href='.\\' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>login</i></a>\n");
            builder.Append("<button onclick=\"document.location='\\in'\" class='w3-bar-item w3-button' ><i class='w3-xxlarge material-icons-outlined'>drafts</i></button>\n");
            builder.Append("<button onclick=\"document.location='\\out'\" class='w3-bar-item w3-button' ><i class='w3-xxlarge material-icons-outlined'>redo</i></button>\n");
            builder.Append("<button onclick=\"document.location='\\overdue'\" class='w3-bar-item w3-button' " + Disabled + "><i class='w3-xxlarge material-icons-outlined'>alarm</i></button>\n");
            builder.Append("<button onclick=\"document.location='\\blocked'\" class='w3-bar-item w3-button' " + Disabled + "><i class='w3-xxlarge material-icons-outlined'>alarm_off</i></button>\n");
            builder.Append("<button onclick=\"document.location='\\shift'\" class='w3-bar-item w3-button' " + Disabled + "><i class='w3-xxlarge material-icons-outlined'>event</i></button>\n");
            builder.Append("<button onclick=\"document.location='\\account'\" class='w3-bar-item w3-button' " + Disabled + "><i class='w3-xxlarge material-icons-outlined'>contact_page</i></button>\n");
            builder.Append("<button onclick=\"document.location='\\'\" class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>person</i></button>\n");
            builder.Append("</div>\n");
            builder.Append("<center>\n");
            builder.Append("<div class='w3-container w3-cyan'>\n");
            builder.Append("<h1>MelBox2 - " + htmlTitle + "</h1>\n</div>\n\n");

            return builder.ToString();
        }


        private static string ToHTML_Table(DataTable dt)
        {
            //Quelle: https://stackoverflow.com/questions/19682996/datatable-to-html-table

            if (dt.Rows.Count == 0) return "FEHLER SQL: Ergebnis leer";

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

        //BAUSTELLE: 'Ein Aufrufziel hat einen Ausnahmefehler verursacht.'
        private static string ToHTML_AccountForm(DataTable dt)
        {
            if (dt.Rows.Count == 0) return "FEHLER SQL: Ergebnis leer";

            //Quelle: https://stackoverflow.com/questions/19682996/datatable-to-html-table

            DataTable dtCompanys = Sql.GetAllCompanys();

            StringBuilder builder = new StringBuilder();
            builder.Append(HtmlHead(dt.TableName));

            builder.Append("<form action='/account' class='w3-container w3-card w3-light-grey w3-text-cyan w3-third w3-display-middle'>\n");
            builder.Append("<h2>Kontaktdaten</h2>\n");

            //ContactId hier anzeigen?
            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-col' style='width:50px'><i class='w3-xxlarge material-icons-outlined'>push_pin</i></div>\n");
            builder.Append(" <div class='w3-rest'>\n");
            builder.Append("  <input class='w3-input w3-border' name='contactid' type='text' placeholder='Eindeutige Nummer' value='" + dt.Rows[0]["ContactId"].ToString() +  "' disabled>\n");
            builder.Append(" </div>\n");
            builder.Append("</div>\n\n");

            string name = dt.Rows[0]["Name"].ToString().Replace("ä", "&auml;").Replace("ö", "&ouml;").Replace("ü", "&uuml;");

            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-col' style='width:50px'><i class='w3-xxlarge material-icons-outlined'>person</i><span>*</span></div>\n");
            builder.Append(" <div class='w3-rest'>\n");
            builder.Append("  <input class='w3-input w3-border' name='name' type='text' placeholder='Name (wird f&uuml;r andere sichtbar angezeigt)'  value='" + name + "'>\n");
            builder.Append(" </div>\n");
            builder.Append("</div>\n\n");

            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-col' style='width:50px'><i class='w3-xxlarge material-icons-outlined'>vpn_key</i><span>*</span></div>\n");
            builder.Append(" <div class='w3-rest'>\n");
            builder.Append("  <input class='w3-input w3-border' name='password' type='password' placeholder='Passwort (Bei Verlust muss ein neuer Account angelegt werden)' >");
            builder.Append(" </div>\n");
            builder.Append("</div>\n\n");

            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append("<div class='w3-col' style='width:50px'><i class='w3-xxlarge material-icons-outlined'>work</i><span>*</span></div>\n");
            builder.Append("<div class='w3-rest'>\n");
            builder.Append("  <select class='w3-select w3-border' name='company'>\n");
           // builder.Append("   <option value='" + dt.Rows[0]["CompanyId"] + "' selected>" + dt.Rows[0]["CompanyName"] + "</option>\n");

            foreach (DataRow row in dtCompanys.Rows)
            {
                string companyName = row["Name"].ToString().Replace("ä", "&auml;").Replace("ö", "&ouml;").Replace("ü", "&uuml;");

                builder.Append("   <option value='" + row["Id"] + "' ");
                builder.Append(dt.Rows[0]["CompanyId"] == row["Id"] ? "selected" : string.Empty);
                builder.Append(">" + companyName + "</option>\n"); 
            }

            builder.Append("  </select>\n");
            builder.Append("</div>\n</div>\n");

            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-col' style='width:50px'><i class='w3-xxlarge material-icons-outlined'>email</i></div>\n");
            builder.Append(" <div class='w3-rest'>\n");
            builder.Append("  <input class='w3-input w3-border' name='email' type='text' placeholder='E-Mail' value='" + dt.Rows[0]["Email"] + "'>");
            builder.Append(" </div>\n");
            builder.Append("</div>\n\n");

            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-col' style='width:50px'><i class='w3-xxlarge material-icons-outlined'>phone</i></div>\n");
            builder.Append(" <div class='w3-rest'>\n");
            builder.Append("  <input class='w3-input w3-border' name='phone' type='text' placeholder='Mobiltelefonnummer f&uuml;r SMS' value='+" + dt.Rows[0]["Phone"] + "'>");
            builder.Append(" </div>\n");
            builder.Append("</div>\n\n");

            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-col' style='width:50px'><i class='w3-xxlarge material-icons-outlined'>home</i></div>\n");
            builder.Append(" <div class='w3-rest'>\n");
            builder.Append("  <input class='w3-check' type='checkbox' name='sendway' value='" + dt.Rows[0]["SendWay"] + "' ");
            builder.Append( (int.Parse(dt.Rows[0]["SendWay"].ToString()) == 1) ? "ckecked" : string.Empty); 
            builder.Append(">\n <label>SMS</label>\n");
            builder.Append("  <input class='w3-check' type='checkbox' name='sendway' ");
            builder.Append( (int.Parse(dt.Rows[0]["SendWay"].ToString()) == 2) ? "ckecked" : string.Empty);
            builder.Append(">\n <label>Email</label>\n");
            builder.Append("</div>\n</div>\n\n");

            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append("<input class'w3-button w3-teal' type='button' value='Speichern' disabled>\n");
            builder.Append("</div>\n\n");

            builder.Append("</form>");

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

        //TODO: /timetable

        //TODO: /account
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/account")]
        public IHttpContext ShowMelBoxAccount(IHttpContext context)
        {

            //Tabelle der letzten ausgegangenen Nachrichten abrufen, formatieren und wegsenden
            var word = ToHTML_AccountForm(Sql.GetContactInfoView(LogedInContactId));
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

        //TODO: /register über Seite /account?

        [RestRoute]
        public IHttpContext MainMenu(IHttpContext context)
        {
            #region Name & Passwort aus POST auslesen
            var payload = context.Request.Payload;
            string name = string.Empty;
            string password = string.Empty;

            if (payload.Length > 0)
            {
                string[] args = payload.Split('&');

                foreach (string arg in args)
                {
                    string[] pair = arg.Split('=');

                    switch (pair[0])
                    {
                        case "name":
                            name = pair[1].Replace('+', ' ');
                            break;
                        case "password":
                            password = pair[1];
                            break;
                    }
                }
            }
            #endregion

            string registerHtml = string.Empty;
            if (name.Length > 0)
            {
                LogedInContactId = Sql.GetContactIdFromLogin(name, password);

                if (LogedInContactId == 0)
                    username = string.Empty;
                else
                    username = name;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(HtmlHead("Meldesystem"));
            builder.Append("<h2>St&ouml;rmeldesystem f&uuml;r Kreutztr&auml;ger K&auml;ltetechnik</h2>\n");
            builder.Append("<p>&Uuml;bersicht der empfangen und gesendeten Nachrichten,<br>Organisation der Bereitschaft</p>");

            builder.Append("<div class='w3-card w3-quarter w3-display-middle'>\n");
            builder.Append("  <div class='w3-container w3-cyan'>\n");
            builder.Append("    <h2>LogIn</h2>\n");
            builder.Append("  </div>\n");
            builder.Append("  <form class='w3-container' action='/login' method='post'>\n");
            builder.Append("    <label class='w3-text-grey'><b>Name</b></label>\n");
            builder.Append("    <input class='w3-input w3-border w3-sand' name='name' type='text'></p>\n");
            builder.Append("    <label class='w3-text-grey'><b>Passwort</b></label>\n");
            builder.Append("    <input class='w3-input w3-border w3-sand' name='password' type='password'></p>\n");
            builder.Append("    <p>\n");
            builder.Append("    <button class='w3-btn w3-teal'>LogIn</button>\n");

            if (name.Length > 0)
            {
                if (LogedInContactId == 0)
                {
                    builder.Append("<input class='w3-btn w3-teal' type='submit' formaction ='/register' value ='Neu Registrieren'></p>\n<p>Name oder Passwort d&uuml;rfen nicht leer sein.");
                    builder.Append("<div class='w3-panel w3-pale-yellow'>\n<h3>LogIn fehlgeschlagen!</h3> ");
                    builder.Append("<p>Das eingegebene Passwort war falsch oder <br> der Benutzer &quot;" + name + "&quot; ist nicht registriert.</p></div>");
                }
                else
                {
                    builder.Append("<div class='w3-panel w3-pale-green'>\n<h3>LogIn erfolgreich!</h3> ");
                    builder.Append("<p>Wilkommen &quot;" + name + "&quot;.</p></div>");
                }
            }
            builder.Append(registerHtml);
            builder.Append("  </p></form>\n");
            builder.Append("</div>\n");
            //TEST
            //builder.Append("<div>" + payload + "</div>\n");

            builder.Append("</center>\n</body>\n</html>");

            context.Response.SendResponse(builder.ToString());
            return context;
        }
    }


}
