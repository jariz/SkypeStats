using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Data;

namespace SkypeStats.Stats
{
    class Daily : Stat
    {
        public override string Name
        {
            get
            {
                return "Messages daily";
            }
        }

        public override int Icon
        {
            get
            {
                return 2;
            }
        }

        OrderedDictionary daily = new OrderedDictionary();

        public override void RunStep(DataRow Row)
        {
            DateTime timestamp = Core.Epoch.AddSeconds(Convert.ToInt32(Row["timestamp"]));
            if (daily.Contains(timestamp.Date))
                daily[timestamp.Date] = ((int)daily[timestamp.Date]) + 1;
            else daily.Add(timestamp.Date, 1);
            

            //JBox.Out.WriteLine(timestamp.ToLongDateString() + " " + timestamp.ToLongTimeString());
        }

        public override string[] RequiredColumns()
        {
            return new string[] { "timestamp" };
        }

        public override System.Windows.Forms.Control Render(int w, int h)
        {
            return new DailyControl(daily, w, h);
        }
    }
}
