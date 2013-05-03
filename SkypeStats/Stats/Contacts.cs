using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;
using System.IO;
using SkypeStats.Stats.Controls;

namespace SkypeStats.Stats
{
    class Contacts : Stat
    {
        public override string Name
        {
            get
            {
                return "Contacts";
            }
        }

        public override int Icon
        {
            get
            {
                return 3;
            }
        }

        DataTable table = new DataTable();

        public override void RunContactStep(System.Data.DataRow Row)
        {
            table = Row.Table;
        }

        public override string[] RequiredContactColumns()
        {
            return new string[] { "languages", "country", "hex(avatar_image)", "id", "skypename", "displayname" };
        }

        public override System.Windows.Forms.Control Render(int w, int h)
        {
            return new ContactControl(table);
        }
    }
}
