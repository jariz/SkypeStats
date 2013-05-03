using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Drawing;

namespace SkypeStats
{
    class Stat
    {
        public virtual void RunMessageStep(DataRow Row)
        {

        }

        public virtual void RunConversationStep(DataRow Row)
        {

        }

        public virtual void RunContactStep(DataRow Row)
        {

        }

        public virtual void Init()
        {

        }

        public virtual Control Render(int w, int h)
        {
            Panel pan = new Panel();
            Label label = new Label();
            label.Text = "This statistic does not have a Render function :(";
            pan.Size = new System.Drawing.Size(w, h);
            label.AutoSize = true;
            label.Location = new Point((w / 2) - label.Width, (h / 2) - label.Height);
            pan.Controls.Add(label);
            return pan;
        }

        public virtual string Name
        {
            get
            {
                return "Invalid statistic";
            }
        }

        public virtual int Icon
        {
            get
            {
                return 0;
            }
        }

        public virtual string[] RequiredMessageColumns()
        {
            return new string[] { };
        }

        public virtual string[] RequiredConversationColumns()
        {
            return new string[] { };
        }

        public virtual string[] RequiredContactColumns()
        {
            return new string[] { };
        }
    }
}
