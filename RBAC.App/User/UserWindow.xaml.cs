using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using RBAC.lib;
using RBAC.lib.Model;

namespace RBAC.App.User
{
    /// <summary>
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : MetroWindow
    {
        public RoleBasedAccessControlSystem access;

        public UserWindow()
        {
            InitializeComponent();
            access = new RoleBasedAccessControlSystem(false);
            TreeViewModel model = new TreeViewModel(access, 0);
            TreeView.DataContext = model;
            ActiveRoleGrid.ItemsSource = access.active_roles;
            ActivePermissionGrid.ItemsSource = access.active_permissions;
        }

        private void btn_activate_click(object sender, RoutedEventArgs e)
        {
            if (TreeView.SelectedItem == null)
            {
                MessageBox.Show("需选中数据表中的数据项");
            }
            else
            {
                try
                {
                    TreeNode node = TreeView.SelectedItem as TreeNode;
                    access.Acitvate(new RoleModel(node.Id, node.Name));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void selection_changed(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            int rid = Convert.ToInt32(((TreeNode)TreeView.SelectedItem).Id);
            RolePermissionsGrid.ItemsSource = access.getRolePermissions(rid);
        }

        private void btn_logout_click(object sender, RoutedEventArgs e)
        {
            access.Logout();
            Login window = new Login();
            window.Show();
            this.Close();
        }
    }
}
