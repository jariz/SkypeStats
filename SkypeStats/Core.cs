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
        public static DB Database;
        public static List<Stat> Stats = new List<Stat>();
        public static DateTime AnalysisStart;
        public static DateTime AnalysisFinish;
        public static List<string> RequiredColumns;

        public static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static int MessageCount = 0;

        public static void Init()
        {
            Out.WritePlain("SkypeStats by JariZ");
            Out.WriteBlank();

            AddStats();

            Out.WriteLine("Searching for skype databases...");
            string[] Accounts = GetAccounts();

            TaskDialog chooser = new TaskDialog();
            chooser.InstructionText = "The following accounts were found on this computer";
            chooser.Text = "SkypeStats has found several accounts, which one do you want to analyse?";
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
            chooser.Show();

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

            Out.WriteDebug("Init progressdialog...");
            ProgressDialog progress = new ProgressDialog(IntPtr.Zero);
            progress.Title = "SkypeStats";
            progress.Line1 = "Initializing...";
            progress.Line2 = " ";
            progress.Line3 = " ";
            progress.ShowDialog();

            //wait for progdiag to show up
            Thread.Sleep(1000);

            Out.WriteDebug("Counting messages....");
            progress.Line1 = "Counting messages...";
            progress.Value = 1337;
            MessageCount = Convert.ToInt32(Database.ExecuteScalar("SELECT COUNT(*) FROM Messages"));
            Out.WriteLine(MessageCount + " messages found!");

            AnalysisStart = DateTime.Now;
            Out.WriteLine("Analysis started @ " + AnalysisStart.ToLongTimeString());
            progress.Line1 = "Analysing messages...";
            progress.Maximum = Convert.ToUInt32(MessageCount);
            progress.Value = 0;

            int limit = 0;
            int step = 10000;
            string columns = Columns();
            Out.WriteDebug("Using " + columns + " columns.");
            while (limit < MessageCount)
            {
                string query = string.Format("SELECT {0} FROM Messages LIMIT {1},{2}", columns, limit, limit + step);
                Out.WriteDebug("[QUERY] " + query);
                DataTable dt = Database.GetDataTable(query);

                foreach (DataRow row in dt.Rows)
                {
                    foreach (Stat stat in Stats)
                    {
                        stat.RunStep(row);
                    }
                }


                limit += step;
                progress.Value += Convert.ToUInt32(step);
            }

            AnalysisFinish = DateTime.Now;
            DateTime difference = new DateTime(AnalysisFinish.Ticks -  AnalysisStart.Ticks);

            Out.WriteLine(string.Format("Analysis finished in {0}s {1}ms", difference.Second, difference.Millisecond));

            progress.CloseDialog();

            System.Windows.Forms.Application.Run(new SkypeStats());
        }

        static string Columns()
        {
            RequiredColumns = new List<string>();
            foreach (Stat stat in Stats)
            {
                RequiredColumns.AddRange(stat.RequiredColumns());
            }

            StringBuilder columns = new StringBuilder();
            foreach (string column in RequiredColumns)
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
    }
}
