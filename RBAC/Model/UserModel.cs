using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBAC.lib.Model
{
    public class UserModel
    {
        public int uid { get; set; }
        public string name { get; set; }
        public string password { get; set; }

        public UserModel(int _uid)
        {
            uid = _uid;
        }

        public UserModel(int _uid, string _name)
        {
            uid = _uid;
            name = _name;
        }

        public UserModel(int _uid, string _name, string _password)
        {
            uid = _uid;
            name = _name;
            password = _password;
        }
    }
}
