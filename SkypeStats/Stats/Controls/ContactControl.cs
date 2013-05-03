using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SkypeStats.Stats.Controls
{
    public partial class ContactControl : UserControl
    {
        string magic = "FFD8FFE0";

        public Image DoMagic(string raw)
        {
            try
            {
                if (raw.Length == 0) throw new Exception(); //dbnull, abort
                //search for the mysterious 'magic string' http://superuser.com/questions/54021/where-does-skype-save-my-contacts-avatars-in-linux
                string[] rubbish = raw.Split(new string[] { magic }, StringSplitOptions.None);
                if (rubbish.Length != 2) throw new Exception(); //didn't find magic string, abort
                string hexfinal = magic + rubbish[1];
                byte[] jpg = Enumerable.Range(0, hexfinal.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hexfinal.Substring(x, 2), 16)).ToArray();
                MemoryStream ramstream = new MemoryStream(jpg);
                return Image.FromStream(ramstream);
            }
            catch
            {
                return null;
            }
        }

        public ContactControl(DataTable table)
        {
            InitializeComponent();

            foreach (DataRow Row in table.Rows)
            {
                Image image;

                string raw = (string)Row["hex(avatar_image)"];
                image = DoMagic(raw);
                if (image == null)
                    image = new Bitmap(96, 96);

                dataGridView1.Rows.Add(Convert.ToInt32(Row["id"]), image, Row["skypename"], Row["displayname"]);
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
                row.Height = 96;
        }

        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            QueryBox(true);

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

            int id = (int)row.Cells["id"].Value;

            new System.Threading.Thread(QueryThread).Start(id);
        }

        void QueryBox(bool show)
        {
            foreach (Control x in panel1.Controls)
            {
                x.Visible = !show;
            }

            dataGridView1.Enabled = !show;

            //position qbox
            querybox.Location = new Point((panel1.Size.Width / 2) - (querybox.Width / 2), (panel1.Size.Height / 2) - (querybox.Height / 2));
            querybox.Visible = show;
        }

        void QueryThread(object idd)
        {
            int id = (int)idd;
            DB DB = Core.Database;

            DataTable dt = DB.GetDataTable(string.Format("SELECT *, hex(profile_attachments) FROM Contacts WHERE id = {0}", id));
            if (dt.Rows.Count != 1)
            {
                Microsoft.WindowsAPICodePack.Dialogs.TaskDialog.Show("Unable to recieve contact details");
                return;
            }

            DataRow row = dt.Rows[0];

            this.Invoke(new Filler(FillProfile), row);
        }

        delegate void Filler(DataRow contactinformation);

        void FillProfile(DataRow contactinformation)
        {
            try
            {
                skypename.Text = Convert.ToString(contactinformation["skypename"]);
                display.Text = Convert.ToString(contactinformation["displayname"]);
                string bday = Convert.ToString(contactinformation["birthday"]);
                if (contactinformation["birthday"].GetType() != typeof(DBNull) && bday.Length == 8)
                    birthday.Text = new DateTime(Convert.ToInt32(bday.Substring(0, 4)), Convert.ToInt32(bday.Substring(4, 2)), Convert.ToInt32(bday.Substring(6, 2))).ToLongDateString();
                else birthday.Text = "";

                string raw = (string)contactinformation["hex(profile_attachments)"];
                Image image = DoMagic(raw);
                if (image == null)
                    image = new Bitmap(96, 96);
                panel2.BackgroundImage = image;
            }
            catch
            {
                MessageBox.Show("Unable to parse contact information", "Woops", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            QueryBox(false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "JPG files (*.jpg)|";
            sf.Title = "Save HQ profile picture as...";
            if (sf.ShowDialog() != DialogResult.OK)
                return;
            try
            {
                DoMagic(Core.Database.ExecuteScalar(string.Format("SELECT hex(profile_attachments) FROM contacts WHERE id = {0}", (int)dataGridView1.SelectedRows[0].Cells["id"].Value))).Save(sf.FileName + ".jpg");
                System.Diagnostics.Process.Start(sf.FileName + ".jpg");
            }
            catch(Exception z)
            {
                Microsoft.WindowsAPICodePack.Dialogs.TaskDialog.Show(z.ToString(), "Unable to save picture");
            }
        }
    }
}
