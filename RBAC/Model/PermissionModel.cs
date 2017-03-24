using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBAC.lib.Model
{
    public class PermissionModel
    {
        public int pid { get; set; }
        public string name { get; set; }

        public PermissionModel(int _pid)
        {
            pid = _pid;
        }

        public PermissionModel(int _pid, string _name)
        {
            pid = _pid;
            name = _name;
        }
    }
}
