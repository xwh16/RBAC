using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBAC.lib.Model
{
    public class RoleRelationModel
    {
        public int id { get; set; }
        public int parent { get; set; }
        public string parentname { get; set; }
        public int child { get; set; }
        public string childname { get; set; }

        public RoleRelationModel(int _id, int _parent, string _parentname, int _child, string _childname)
        {
            id = _id;
            parent = _parent;
            parentname = _parentname;
            child = _child;
            childname = _childname;
        }
    }
}
