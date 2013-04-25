using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;

namespace SkypeStats
{
    public partial class DailyControl : UserControl
    {
        public int Year = 0;
        public int Month = 0;

        OrderedDictionary Days;
        List<DateTime> DateList;

        public DailyControl(OrderedDictionary Days, int w, int h)
        {
            this.Days = Days;
            InitializeComponent();

            this.Size = new Size(w, h);

            DateTime[] Keys = new DateTime[Days.Keys.Count];
            Days.Keys.CopyTo(Keys, 0);
            List<DateTime> KeysList = new List<DateTime>(Keys);
            DateList = KeysList;

            DateTime Min = KeysList[0];
            DateTime Max = KeysList[KeysList.Count-1];

            Month = Max.Month;
            Year = Max.Year;

            //set up datetimepicker
            dateTimePicker1.MinDate = Min;
            dateTimePicker1.MaxDate = Max;
            dateTimePicker1.Value = Max;
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "MM/yyyy";
            dateTimePicker1.ShowUpDown = true;

            //render da graphz
            comboBox1.Text = "Monthly";
            Render(Max, RT);
        }

        public enum RenderType { Month, Year };
        RenderType RT = RenderType.Month;

        public void Render(DateTime RenderDate, RenderType Type)
        {
            chart1.Series[0].Points.Clear();

            int index = -1;
            switch (Type)
            {
                case RenderType.Month:
                    foreach (DateTime date in DateList)
                    {
                        index++;
                        if (date.Month == RenderDate.Month && date.Year == RenderDate.Year)
                        {
                            chart1.Series[0].Points.AddY(Days[index]);
                        }
                    }

                    break;

                case RenderType.Year:

                    foreach (DateTime date in DateList)
                    {
                        index++;
                        if (date.Year == RenderDate.Year)
                        {
                            chart1.Series[0].Points.AddY(Days[index]);
                        }
                    }
                    break;
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            Render(dateTimePicker1.Value, RT);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].Area3DStyle.Enable3D = checkBox1.Checked;
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            switch (comboBox1.Text)
            {
                case "Monthly":
                    dateTimePicker1.CustomFormat = "MM/yyyy";
                    RT = RenderType.Month;
                    break;
                case "Yearly":
                    dateTimePicker1.CustomFormat = "yyyy";
                    RT = RenderType.Year;
                    break;
            }

            //rerender
            Render(dateTimePicker1.Value, RT);
        }

    }
}
