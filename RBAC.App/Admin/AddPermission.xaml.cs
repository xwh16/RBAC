using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using RBAC.lib;
using RBAC.lib.Model;

namespace RBAC.App.Admin
{
    /// <summary>
    /// AddGeneric.xaml 的交互逻辑
    /// </summary>
    public partial class AddPermission : MetroWindow
    {
        AdminWindow parent;

        public AddPermission(AdminWindow window)
        {
            parent = window;
            InitializeComponent();
        }

        private void btn_add_click(object sender, RoutedEventArgs e)
        {
            try
            {
                PermissionModel perm = new PermissionModel(0, name.Text);
                parent.access.AddPermission(perm, info.Text);
                MessageBox.Show("添加成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Close();
        }
    }
}
