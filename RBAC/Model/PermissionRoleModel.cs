using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBAC.lib.Model
{
    public class PermissionRoleModel
    {
        public int id { get; set; }
        public int pid { get; set; }
        public string permissionname { get; set; }
        public int rid { get; set; }
        public string rolename { get; set; }

        public PermissionRoleModel(int _id, int _pid, string _permissionname, int _rid, string _rolename)
        {
            id = _id;
            pid = _pid;
            permissionname = _permissionname;
            rid = _rid;
            rolename = _rolename;
        }
    }
}
