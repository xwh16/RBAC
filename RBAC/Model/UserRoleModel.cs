using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBAC.lib.Model
{
    public class UserRoleModel
    {
        public int id { get; set; }
        public int uid { get; set; }
        public string username { get; set; }
        public int rid { get; set; }
        public string rolename { get; set; }

        public UserRoleModel(int _id, int _uid, string _username, int _rid, string _rolename)
        {
            id = _id;
            uid = _uid;
            username = _username;
            rid = _rid;
            rolename = _rolename;
        }
    }
}
