using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;

namespace SkypeStats.Stats.Controls
{
    public partial class DailyControl : UserControl
    {
        public int Year = 0;
        public int Month = 0;

        OrderedDictionary In;
        OrderedDictionary Out;
        List<DateTime> DateList;

        public DailyControl(OrderedDictionary _in, OrderedDictionary _out, int w, int h)
        {
            this.In = _in;
            this.Out = _out;

            InitializeComponent();

            this.Size = new Size(w, h);

            //collect data to calculate min and max
            DateTime[] Keys = new DateTime[In.Keys.Count + Out.Keys.Count];
            In.Keys.CopyTo(Keys, 0);
            Out.Keys.CopyTo(Keys, In.Keys.Count);
            List<DateTime> KeysList = new List<DateTime>(Keys);

            DateTime Min = KeysList[0];
            DateTime Max = KeysList[KeysList.Count-1];

            Month = Max.Month;
            Year = Max.Year;

            //build date list with all dates between min & max
            DateList = new List<DateTime>();
            for (DateTime date = Min; date <= Max; date = date.AddDays(1)) DateList.Add(date);

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

            //misc settings
            //propertyGrid1.;
        }

        public enum RenderType { Month, Year };
        RenderType RT = RenderType.Month;

        public void Render(DateTime RenderDate, RenderType Type)
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();

            int index = -1;
            switch (Type)
            {
                case RenderType.Month:
                    foreach (DateTime date in DateList)
                    {
                        index++;

                        if (date.Year == RenderDate.Year && date.Month == RenderDate.Month)
                        {
                            try
                            {
                                chart1.Series[0].Points.AddY(In[date]);
                            }
                            catch
                            {
                                chart1.Series[0].Points.AddY(0);
                            }


                            try
                            {
                                chart1.Series[1].Points.AddY(Out[date]);
                            }
                            catch
                            {
                                chart1.Series[1].Points.AddY(0);
                            }
                        }
                    }

                    break;

                case RenderType.Year:

                    foreach (DateTime date in DateList)
                    {
                        index++;
                        if (date.Year == RenderDate.Year)
                        {
                            try
                            {
                                chart1.Series[0].Points.AddY(In[date]);
                            }
                            catch
                            {
                                chart1.Series[0].Points.AddY(0);
                            }


                            try
                            {
                                chart1.Series[1].Points.AddY(Out[date]);
                            }
                            catch
                            {
                                chart1.Series[1].Points.AddY(0);
                            }
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
