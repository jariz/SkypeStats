using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Collections;
using System.Windows.Forms;

namespace SkypeStats.Stats
{
    class Top20 : Stat
    {
        public Dictionary<string, int> Scoreboard = new Dictionary<string, int>();

        public override void RunStep(DataRow Row)
        {
            if (!Scoreboard.ContainsKey((string)Row["author"]))
                Scoreboard.Add((string)Row["author"], 1);
            else Scoreboard[(string)Row["author"]] += 1;
        }

        public override Control Render(int w, int h)
        {
            return base.Render(w, h);
        }

        public override string Name
        {
            get
            {
                return "Top 20";
            }
        }

        public override int Icon
        {
            get
            {
                return 1;
            }
        }

        public override string[] RequiredColumns()
        {
            return new string[] { "author" };
        }
    }
}
