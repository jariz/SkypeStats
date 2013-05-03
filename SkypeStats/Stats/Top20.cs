using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Collections;
using System.Windows.Forms;
using SkypeStats.Stats.Controls;

namespace SkypeStats.Stats
{
    class Top20 : Stat
    {

        Dictionary<int, string> /* id, identity */ Convos = new Dictionary<int, string>();
        Dictionary<int, int> /* id, messages */ In;
        Dictionary<int, int> /* id, messages */ Out;

        public override void RunMessageStep(DataRow Row)
        {

            if (In == null && Out == null)
            {
                In = new Dictionary<int, int>();
                Out = new Dictionary<int, int>();
                foreach (KeyValuePair<int, string> item in Convos)
                {
                    In.Add(item.Key, 0);
                    Out.Add(item.Key, 0);
                }
            }

            int cid = Convert.ToInt32(Row["convo_id"]);

            if ((string)Row["author"] == Core.Account && Out.ContainsKey(cid))
                Out[cid] += 1;
            else if (Out.ContainsKey(cid)) In[cid] += 1;
        }

        public override void RunConversationStep(DataRow Row)
        {
            Convos.Add(Convert.ToInt32(Row["id"]), (string)Row["identity"]);
        }

        public override Control Render(int w, int h)
        {
            return new Top20Control(Convos, In, Out);
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

        public override string[] RequiredMessageColumns()
        {
            return new string[] { "author", "convo_id" };
        }

        public override string[] RequiredConversationColumns()
        {
            return new string[] { "id", "identity" };
        }
    }
}
