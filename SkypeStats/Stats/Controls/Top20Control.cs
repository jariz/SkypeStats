using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SkypeStats.Stats.Controls
{
    public partial class Top20Control : UserControl
    {
        public Top20Control(Dictionary<int, string> Convos, Dictionary<int, int> In, Dictionary<int,int> Out)
        {
            InitializeComponent();

            dataGridView1.Rows.Clear();

            int index = 0;
            foreach (KeyValuePair<int, string> convo in  Convos.OrderByDescending(key => Out[key.Key] + In[key.Key]))
            {
                index++;
                if (index > 20) break;
                dataGridView1.Rows.Add(index, convo.Value, In[convo.Key], Out[convo.Key], Out[convo.Key] + In[convo.Key]);
            }

            //dataGridView1.Sort(dataGridView1.Columns[3], ListSortDirection.Descending);
        }
    }
}
