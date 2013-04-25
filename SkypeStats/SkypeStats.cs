using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using JBox;

namespace SkypeStats
{
    public partial class SkypeStats : Form
    {
        public SkypeStats()
        {
            InitializeComponent();

            RenderStatistics();
        }

        List<Control> RenderedStats = new List<Control>();

        public void Switch(int index)
        {
            ClearHolder();

            if (RenderedStats.Count > index)
                StatHolder.Controls.Add(RenderedStats[index]);
            else TaskDialog.Show("Invalid index specified", "Something went wrong while getting this stat's content", "Woops");
        }

        public void ClearHolder()
        {
            //remove currently displaying stats (if any)
            foreach (Control stat in StatHolder.Controls)
            {
                //bai
                StatHolder.Controls.Remove(stat);
            }
        }

        public void RenderStatistics()
        {
            Out.WriteLine("Rendering stats...");
            ClearHolder();

            //fill renderedstats list and render stats
            foreach (Stat stat in Core.Stats)
            {
                RenderedStats.Add(stat.Render(StatHolder.Width, StatHolder.Height));
            }

            if (RenderedStats.Count > 0)
            {
                listView1.Clear();
                foreach (Stat stat in Core.Stats)
                {
                    listView1.Items.Add(stat.Name, stat.Icon);
                }

                Switch(0);
            }
            else Out.WriteDebug("It seems like there are no stats to display?");
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listView1.SelectedIndices.Count > 0)
                Switch(listView1.SelectedIndices[0]);
        }
    }
}
