using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace RBAC.lib.Model
{
    public class RoleActivation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int rid { get; set; }
        public string rolename { get; set; }
        public string method
        {
            get
            {
                if (_inherit == true)
                    return "角色通过继承获得";
                else
                    return "角色通过直接激活获得";
            }
        }

        private bool _inherit;
        public bool isHerit
        {
            get
            {
                return _inherit;
            }
            set
            {
                _inherit = value;
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("isHerit"));
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("method"));
            }
        }

        public RoleActivation(int _rid, string _rolename, bool _isherit)
        {
            rid = _rid;
            rolename = _rolename;
            _inherit = _isherit;
        }
    }
}
