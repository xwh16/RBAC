using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBAC.lib.Model
{
    public class RoleModel
    {
        public int rid;
        public string name;

        public RoleModel(int _rid)
        {
            rid = _rid;
        }

        public RoleModel(int _rid, string _name)
        {
            rid = _rid;
            name = _name;
        }
    }
}
