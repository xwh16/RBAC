using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBAC.lib.Model
{
    public class ExclusionModel
    {
        public int id { get; set; }
        public int rid1 { get; set; }
        public string rid1name { get; set; }
        public int rid2 { get; set; }
        public string rid2name { get; set; }

        public ExclusionModel(int _id, int _rid1, string _rid1name, int _rid2, string _rid2name)
        {
            id = _id;
            rid1 = _rid1;
            rid1name = _rid1name;
            rid2 = _rid2;
            rid2name = _rid2name;
        }

        public ExclusionModel(int _id, int _rid1, int _rid2)
        {
            id = _id;
            rid1 = _rid1;
            rid2 = _rid2;
        }

        public ExclusionModel(int _id)
        {
            id = _id;
        }
    }
}
