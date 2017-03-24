using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace RBAC.lib.Model
{
    public class PermissionActivation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int pid { get; set; }
        public string permissionname { get; set; }
        public string method {
            get
            {
                if (_inherit == true)
                    return "权限通过继承: " + sourcename + " 获得";
                else
                    return "权限通过激活: " + sourcename + " 获得";
            }
        }
        private int source;
        private string sourcename;

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

        public PermissionActivation(int _pid, string _permissionname, bool _method, int _source, string _sourcename)
        {
            pid = _pid;
            permissionname = _permissionname;
            _inherit = _method;
            source = _source;
            sourcename = _sourcename;
        }
    }
}
