using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JBox;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Data.SQLite;
using System.Data;
using SkypeStats.Stats;

namespace SkypeStats
{
    class Core
    {
        public static string SkypeAppData = string.Empty;
        public static string DatabasePath = string.Empty;
        public static string Account = string.Empty;
        public static bool? MaxedOutMode = false;
        public static DB Database;
        public static List<Stat> Stats = new List<Stat>();
        public static DateTime AnalysisStart;
        public static DateTime AnalysisFinish;
        public static List<string> RequiredColumns;
        public static bool ProgDiag = false;
        public static MainWindow Splash;

        public static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static int MessageCount = 0;
        public static int ConvoCount = 0;
        public static int ContactCount = 0;

        public static void Init()
        {
            Console.WindowWidth = Console.LargestWindowWidth - 25;
            Console.WindowHeight = Console.LargestWindowHeight - 25;
            Console.Title = "SkypeStats - Debug Console";

            Out.WritePlain("SkypeStats by JariZ");
            Out.WriteBlank();

            AddStats();

            Out.WriteLine("Searching for skype databases...");
            string[] Accounts = GetAccounts();

            TaskDialog chooser = new TaskDialog();
            chooser.InstructionText = "The following accounts were found on this computer";
            chooser.Text = "SkypeStats has found several accounts, which one do you want to analyse?";
            chooser.FooterCheckBoxText = "Load entire DB into memory and max out cache";
            
            string Account = "";
            foreach (string account in Accounts)
            {
                TaskDialogCommandLink link = new TaskDialogCommandLink(Path.GetFileName(account), Path.GetFileName(account));
                link.Click += delegate(object send, EventArgs args) {
                    Account = ((TaskDialogCommandLink)send).Name;
                    chooser.Close();
                };
                chooser.Controls.Add(link);
            }

            chooser.FooterText = "Loading everything into memory might be faster but requires a lot of RAM, only choose this if you know what you're doing";
            chooser.FooterIcon = TaskDialogStandardIcon.Information;

            chooser.Show();

            MaxedOutMode = chooser.FooterCheckBoxChecked;

            Core.Account = Account;
            DatabasePath = SkypeAppData + "\\" + Account + "\\main.db";
            if (!File.Exists(DatabasePath))
                Failure("Database file not found, restart SkypeStats");

            Out.WriteDebug("DB path is " + DatabasePath + " and exists");

            Out.WriteDebug("Connecting to DB...");
            try
            {
                Database = new DB(DatabasePath);
            }
            catch(Exception z)
            {
                Out.WriteDebug("UNABLE TO CONNECT TO DB:");
                Out.WriteError(z.ToString(), z);
                Failure("Unable to connect to database file: " + z.Message);
            }
            Out.WriteLine("Successfully connected to DB!");

            ProgressDialog progress = null;
            if (ProgDiag)
            {
                Out.WriteDebug("Init progressdialog...");
                progress = new ProgressDialog(IntPtr.Zero);
                progress.Title = "SkypeStats";
                progress.Line1 = "Initializing...";
                progress.Line2 = " ";
                progress.Line3 = " ";
                progress.ShowDialog();

                //wait for progdiag to show up
                Thread.Sleep(1000);
            }
            else
            {
                Thread splash = new Thread(SplashThread);
                splash.SetApartmentState(ApartmentState.STA);
                splash.Start();

                while (Splash == null) { }
            }

            Out.WriteDebug("Counting messages....");
            if (ProgDiag)
            {
                progress.Line1 = "Counting messages...";
                progress.Value = 1337;
            }
            else
            {
                Splash.Status = "Counting messages...";
                Splash.Value = 1337;
            }
            MessageCount = Convert.ToInt32(Database.ExecuteScalar("SELECT COUNT(*) FROM Messages"));
            Out.WriteLine(MessageCount + " messages found!");

            Out.WriteDebug("Counting conversations....");
            if (ProgDiag)
            {
                progress.Line1 = "Counting conversations...";
                progress.Value = 1337;
            }
            else
            {
                Splash.Status = "Counting conversations...";
                Splash.Value = 1337;
            }
            ConvoCount = Convert.ToInt32(Database.ExecuteScalar("SELECT COUNT(*) FROM Conversations"));
            Out.WriteLine(ConvoCount + " conversations found!");

            Out.WriteDebug("Counting contacts....");
            if (ProgDiag)
            {
                progress.Line1 = "Counting contacts...";
                progress.Value = 1337;
            }
            else
            {
                Splash.Status = "Counting contacts...";
                Splash.Value = 1337;
            }
            ContactCount = Convert.ToInt32(Database.ExecuteScalar("SELECT COUNT(*) FROM Contacts"));
            Out.WriteLine(ContactCount + " contacts found!");

            AnalysisStart = DateTime.Now;
            Out.WriteLine("Analysis started @ " + AnalysisStart.ToLongTimeString());

            Out.WriteLine("Analysing contacts...");
            if (ProgDiag)
            {
                progress.Line1 = "Analysing contacts...";
                progress.Maximum = Convert.ToUInt32(ContactCount);
                progress.Value = 0;
            }
            else
            {
                Splash.Status = "Analysing contacts...";
                Splash.Maximum = ContactCount;
                Splash.Value = 0;
            }

            int limit = 0;
            int step = 1000;
            if (MaxedOutMode == true)
                step = int.MaxValue;

            string columns = Columns(ColumnType.Contact);
            Out.WriteDebug("Using " + columns + " columns.");

            while (limit < ContactCount)
            {
                string query = string.Format("SELECT {0} FROM Contacts LIMIT {1},{2}", columns, limit, limit + step);
                DataTable dt = Database.GetDataTable(query);

                foreach (DataRow row in dt.Rows)
                {
                    foreach (Stat stat in Stats)
                    {
                        stat.RunContactStep(row);
                    }
                }


                limit += step;
                if (ProgDiag) progress.Value += Convert.ToUInt32(step);
                else Splash.AddValue(step);
            }

            Out.WriteLine("Analysing conversations...");
            if (ProgDiag)
            {
                progress.Line1 = "Analysing conversations...";
                progress.Maximum = Convert.ToUInt32(ConvoCount);
                progress.Value = 0;
            }
            else
            {
                Splash.Status = "Analysing conversations...";
                Splash.Maximum = ConvoCount;
                Splash.Value = 0;
            }

            limit = 0;
            step = 1000;
            if (MaxedOutMode == true)
                step = int.MaxValue;
                
            columns = Columns(ColumnType.Convo);
            Out.WriteDebug("Using " + columns + " columns.");

            while (limit < ConvoCount)
            {
                string query = string.Format("SELECT {0} FROM Conversations WHERE type = 1 LIMIT {1},{2}", columns, limit, limit + step);
                DataTable dt = Database.GetDataTable(query);

                foreach (DataRow row in dt.Rows)
                {
                    foreach (Stat stat in Stats)
                    {
                        stat.RunConversationStep(row);
                    }
                }


                limit += step;
                if (ProgDiag) progress.Value += Convert.ToUInt32(step);
                else Splash.AddValue(step);
            }


            Out.WriteLine("Analysing messages...");
            if (ProgDiag)
            {
                progress.Line1 = "Analysing messages...";
                progress.Maximum = Convert.ToUInt32(MessageCount);
                progress.Value = 0;
            }
            else
            {
                Splash.Status = "Analysing messages...";
                Splash.Maximum = MessageCount;
                Splash.Value = 0;
            }

            limit = 0;
            step = 10000;
            if (MaxedOutMode == true)
                step = int.MaxValue;
            columns = Columns(ColumnType.Message);
            Out.WriteDebug("Using " + columns + " columns.");
            while (limit < MessageCount)
            {
                string query = string.Format("SELECT {0} FROM Messages LIMIT {1},{2}", columns, limit, limit + step);
                DataTable dt = Database.GetDataTable(query);

                foreach (DataRow row in dt.Rows)
                {
                    foreach (Stat stat in Stats)
                    {
                        stat.RunMessageStep(row);
                    }
                }


                limit += step;
                if (ProgDiag) progress.Value += Convert.ToUInt32(step);
                else Splash.AddValue(step);
            }

            AnalysisFinish = DateTime.Now;
            DateTime difference = new DateTime(AnalysisFinish.Ticks -  AnalysisStart.Ticks);

            Out.WriteLine(string.Format("Analysis finished in {0}s {1}ms", difference.Second, difference.Millisecond));

            if (ProgDiag) progress.CloseDialog();
            else Splash.Kill();

            
            System.Windows.Forms.Application.Run(new SkypeStats());
        }

        static bool run = true;

        [STAThread]
        static void SplashThread(object x)
        {
            Splash = new MainWindow();
            Splash.Show();

            System.Windows.Forms.Application.Run();
        }

        static Dictionary<SplashActionType, object> Queue = new Dictionary<SplashActionType, object>();

        enum SplashActionType { Max, Value, Status, Close };
        static void SplashAction(SplashActionType type, object value)
        {
            Queue.Add(type, value);
        }

        enum ColumnType { Message, Convo, Contact };
        static string Columns(ColumnType type)
        {
            RequiredColumns = new List<string>();
            foreach (Stat stat in Stats)
            {
                switch (type)
                {
                    case ColumnType.Message:
                        RequiredColumns.AddRange(stat.RequiredMessageColumns());
                        break;
                    case ColumnType.Convo:
                        RequiredColumns.AddRange(stat.RequiredConversationColumns());
                        break;
                    case ColumnType.Contact:
                        RequiredColumns.AddRange(stat.RequiredContactColumns());
                        break;
                }
            }

            StringBuilder columns = new StringBuilder();
            foreach (string column in RequiredColumns.Distinct().ToList())
            {
                columns.Append(column);
                columns.Append(",");
            }
            
            return columns.ToString().Substring(0, columns.ToString().Length - 1);
        }

        public static void AddStats()
        {
            Stats.Add(new Top20());
            Stats.Add(new Daily());
            Stats.Add(new Contacts());

            foreach (Stat stat in Stats)
            {
                stat.Init();
            }
        }

        public static void Failure(string message)
        {
            TaskDialog diag = new TaskDialog();
            diag.InstructionText = "Unable to initialize SkypeStats!";
            diag.Text = message;
            diag.Icon = TaskDialogStandardIcon.Error;
            diag.Show();
            Environment.Exit(-1);
        }

        public static string[] GetAccounts()
        {
            List<string> result = new List<string>();
            SkypeAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Skype";
            if (!Directory.Exists(SkypeAppData))
                Failure("Unable to find Skype data directory!\r\nAre you sure Skype is installed?");

            foreach (string dir in Directory.GetDirectories(SkypeAppData))
            {
                if (File.Exists(dir + "\\main.db"))
                    result.Add(dir);
            }
            return result.ToArray();
        }



        public static string[] CountryNames = new string[] {
            "Afghanistan", "Albania", "Algeria", "American Samoa", "Andorra", "Angola", "Anguilla", "Antarctica", 
            "Antigua and Barbuda", "Argentina", "Armenia", "Aruba", "Australia", "Austria", "Azerbaijan", "Bahamas", 
            "Bahrain", "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bermuda", "Bhutan", 
            "Bolivia", "Bosnia and Herzegovina", "Botswana", "Bouvet Island", "Brazil", "British Indian Ocean Territory", 
            "Brunei Darussalam", "Bulgaria", "Burkina Faso", "Burundi", "Cambodia", "Cameroon", "Canada", "Cape Verde", 
            "Cayman Islands", "Central African Republic", "Chad", "Chile", "China", "Christmas Island", "Cocos (Keeling) Islands", 
            "Colombia", "Comoros", "Congo", "Congo, the Democratic Republic of the", "Cook Islands", "Costa Rica", "Cote D'Ivoire", 
            "Croatia", "Cuba", "Cyprus", "Czech Republic", "Denmark", "Djibouti", "Dominica", "Dominican Republic", "Ecuador", 
            "Egypt", "El Salvador", "Equatorial Guinea", "Eritrea", "Estonia", "Ethiopia", "Falkland Islands (Malvinas)", 
            "Faroe Islands", "Fiji", "Finland", "France", "French Guiana", "French Polynesia", "French Southern Territories", 
            "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Gibraltar", "Greece", "Greenland", "Grenada", "Guadeloupe", "Guam", 
            "Guatemala", "Guinea", "Guinea-Bissau", "Guyana", "Haiti", "Heard Island and Mcdonald Islands", 
            "Holy See (Vatican City State)", "Honduras", "Hong Kong", "Hungary", "Iceland", "India", "Indonesia", 
            "Iran, Islamic Republic of", "Iraq", "Ireland", "Israel", "Italy", "Jamaica", "Japan", "Jordan", "Kazakhstan", "Kenya", 
            "Kiribati", "Korea, Democratic People's Republic of", "Korea, Republic of", "Kuwait", "Kyrgyzstan", 
            "Lao People's Democratic Republic", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libyan Arab Jamahiriya", "Liechtenstein", 
            "Lithuania", "Luxembourg", "Macao", "Macedonia, the Former Yugoslav Republic of", "Madagascar", "Malawi", "Malaysia", 
            "Maldives", "Mali", "Malta", "Marshall Islands", "Martinique", "Mauritania", "Mauritius", "Mayotte", "Mexico", 
            "Micronesia, Federated States of", "Moldova, Republic of", "Monaco", "Mongolia", "Montserrat", "Morocco", "Mozambique", 
            "Myanmar", "Namibia", "Nauru", "Nepal", "Netherlands", "Netherlands Antilles", "New Caledonia", "New Zealand", "Nicaragua", 
            "Niger", "Nigeria", "Niue", "Norfolk Island", "Northern Mariana Islands", "Norway", "Oman", "Pakistan", "Palau", 
            "Palestinian Territory, Occupied", "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Pitcairn", "Poland", 
            "Portugal", "Puerto Rico", "Qatar", "Reunion", "Romania", "Russian Federation", "Rwanda", "Saint Helena", 
            "Saint Kitts and Nevis", "Saint Lucia", "Saint Pierre and Miquelon", "Saint Vincent and the Grenadines", "Samoa", 
            "San Marino", "Sao Tome and Principe", "Saudi Arabia", "Senegal", "Serbia and Montenegro", "Seychelles", "Sierra Leone", 
            "Singapore", "Slovakia", "Slovenia", "Solomon Islands", "Somalia", "South Africa", 
            "South Georgia and the South Sandwich Islands", "Spain", "Sri Lanka", "Sudan", "Suriname", "Svalbard and Jan Mayen", 
            "Swaziland", "Sweden", "Switzerland", "Syrian Arab Republic", "Taiwan, Province of China", "Tajikistan", 
            "Tanzania, United Republic of", "Thailand", "Timor-Leste", "Togo", "Tokelau", "Tonga", "Trinidad and Tobago", 
            "Tunisia", "Turkey", "Turkmenistan", "Turks and Caicos Islands", "Tuvalu", "Uganda", "Ukraine", "United Arab Emirates", 
            "United Kingdom", "United States", "United States Minor Outlying Islands", "Uruguay", "Uzbekistan", "Vanuatu", "Venezuela", 
            "Viet Nam", "Virgin Islands, British", "Virgin Islands, U.s.", "Wallis and Futuna", "Western Sahara", "Yemen", "Zambia", 
            "Zimbabwe" };

        public static string[] CountryAbbreviations = new string[] {
            "AF", "AL", "DZ", "AS", "AD", "AO", "AI", "AQ", "AG", "AR", "AM", "AW", "AU", "AT", "AZ", "BS", "BH", "BD", "BB", "BY", 
            "BE", "BZ", "BJ", "BM", "BT", "BO", "BA", "BW", "BV", "BR", "IO", "BN", "BG", "BF", "BI", "KH", "CM", "CA", "CV", "KY", 
            "CF", "TD", "CL", "CN", "CX", "CC", "CO", "KM", "CG", "CD", "CK", "CR", "CI", "HR", "CU", "CY", "CZ", "DK", "DJ", "DM", 
            "DO", "EC", "EG", "SV", "GQ", "ER", "EE", "ET", "FK", "FO", "FJ", "FI", "FR", "GF", "PF", "TF", "GA", "GM", "GE", "DE", 
            "GH", "GI", "GR", "GL", "GD", "GP", "GU", "GT", "GN", "GW", "GY", "HT", "HM", "VA", "HN", "HK", "HU", "IS", "IN", "ID", 
            "IR", "IQ", "IE", "IL", "IT", "JM", "JP", "JO", "KZ", "KE", "KI", "KP", "KR", "KW", "KG", "LA", "LV", "LB", "LS", "LR", 
            "LY", "LI", "LT", "LU", "MO", "MK", "MG", "MW", "MY", "MV", "ML", "MT", "MH", "MQ", "MR", "MU", "YT", "MX", "FM", "MD", 
            "MC", "MN", "MS", "MA", "MZ", "MM", "NA", "NR", "NP", "NL", "AN", "NC", "NZ", "NI", "NE", "NG", "NU", "NF", "MP", "NO", 
            "OM", "PK", "PW", "PS", "PA", "PG", "PY", "PE", "PH", "PN", "PL", "PT", "PR", "QA", "RE", "RO", "RU", "RW", "SH", "KN", 
            "LC", "PM", "VC", "WS", "SM", "ST", "SA", "SN", "CS", "SC", "SL", "SG", "SK", "SI", "SB", "SO", "ZA", "GS", "ES", "LK", 
            "SD", "SR", "SJ", "SZ", "SE", "CH", "SY", "TW", "TJ", "TZ", "TH", "TL", "TG", "TK", "TO", "TT", "TN", "TR", "TM", "TC", 
            "TV", "UG", "UA", "AE", "GB", "US", "UM", "UY", "UZ", "VU", "VE", "VN", "VG", "VI", "WF", "EH", "YE", "ZM", "ZW" };
    }
}
