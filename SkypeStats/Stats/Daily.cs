using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Data;
using SkypeStats.Stats.Controls;

namespace SkypeStats.Stats
{
    class Daily : Stat
    {
        public override string Name
        {
            get
            {
                return "Messages in/out";
            }
        }

        public override int Icon
        {
            get
            {
                return 2;
            }
        }

        OrderedDictionary _in = new OrderedDictionary();
        OrderedDictionary _out = new OrderedDictionary();

        public override void RunMessageStep(DataRow Row)
        {
            DateTime timestamp = Core.Epoch.AddSeconds(Convert.ToInt32(Row["timestamp"]));
            // determine if in/out
            if ((string)Row["author"] == Core.Account)
            {
                if (_out.Contains(timestamp.Date))
                    _out[timestamp.Date] = ((int)_out[timestamp.Date]) + 1;
                else _out.Add(timestamp.Date, 1);
            }
            else
            {
                if (_in.Contains(timestamp.Date))
                    _in[timestamp.Date] = ((int)_in[timestamp.Date]) + 1;
                else _in.Add(timestamp.Date, 1);
            }
            

            //JBox.Out.WriteLine(timestamp.ToLongDateString() + " " + timestamp.ToLongTimeString());
        }

        public override string[] RequiredMessageColumns()
        {
            return new string[] { "timestamp", "author" };
        }

        public override System.Windows.Forms.Control Render(int w, int h)
        {
            return new DailyControl(_in, _out, w, h);
        }
    }
}
