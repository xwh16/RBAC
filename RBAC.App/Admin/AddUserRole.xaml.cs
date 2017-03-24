using System;
using System.Data;
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
    /// AddUserRole.xaml 的交互逻辑
    /// </summary>
    public partial class AddUserRole : MetroWindow
    {
        AdminWindow parent;

        public AddUserRole(AdminWindow window)
        {
            parent = window;
            InitializeComponent();
            this.comboBox.ItemsSource = from table in parent.access.UserTable.AsEnumerable()
                                        select new 
                                        ListItem(
                                            table.Field<string>("name"),
                                            table.Field<int>("uid")
                                        );
            this.comboBox1.ItemsSource = from table in parent.access.RoleTable.AsEnumerable()
                                         select new
                                         ListItem(
                                            table.Field<string>("name"),
                                            table.Field<int>("rid")
                                         );
        }

        private void btn_add_click(object sender, RoutedEventArgs e)
        {
            try
            {
                parent.access.AssignUserRole(
                    new UserModel((comboBox.SelectedItem as ListItem).Value),
                    new RoleModel((comboBox1.SelectedItem as ListItem).Value)
                    );
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
