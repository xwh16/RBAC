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
using System.Windows.Threading;

namespace RBAC.App.Admin
{
    /// <summary>
    /// AdminWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AdminWindow : MetroWindow
    {
        public delegate void TransitionMessage(string msg);
        public event TransitionMessage handler;

        public RoleBasedAccessControlSystem access;

        public AdminWindow()
        {
            InitializeComponent();
            access = new RoleBasedAccessControlSystem(true);
            UserGrid.ItemsSource = access.UserTable.DefaultView;
            RoleGrid.ItemsSource = access.RoleTable.DefaultView;
            PermissionGrid.ItemsSource = access.PermissionTable.DefaultView;
            ExclusionGrid.ItemsSource = access.ExclusionTable.DefaultView;

            UserRoleGrid.ItemsSource = access.getURAView();
            PermissionRoleGrid.ItemsSource = access.getPRAView();
            RoleRoleGrid.ItemsSource = access.getRRAView();

            access.handler += Tick;
            handler += Tick;
        }

        void Tick(string msg)
        {
            transitioning.Content = new TextBlock { Text = msg, SnapsToDevicePixels = true };
        }

        #region tab1
        private void tab1_load(object sender, RoutedEventArgs e)
        {
            access.GetUesrList();
            access.GetRoleList();
            access.GetPermissionList();
        }

        private void btn_add_user_click(object sender, RoutedEventArgs e)
        {
            AddUser window = new AddUser(this);
            window.Show();
        }

        private void btn_delete_user_click(object sender, RoutedEventArgs e)
        {
            if (UserGrid.SelectedItem == null)
            {
                MessageBox.Show("需选中数据表中的数据项");
            }
            else
            {
                try
                {
                    int id = Convert.ToInt32(((DataRowView)UserGrid.SelectedItem).Row.ItemArray[0]);
                    access.DeleteUser(new UserModel(id));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btn_add_role_click(object sender, RoutedEventArgs e)
        {
            AddRole window = new AddRole(this);
            window.Show();
        }

        private void btn_delete_role_click(object sender, RoutedEventArgs e)
        {
            if (RoleGrid.SelectedItem == null)
            {
                MessageBox.Show("需选中数据表中的数据项");
            }
            else
            {
                try
                {
                    int id = Convert.ToInt32(((DataRowView)RoleGrid.SelectedItem).Row.ItemArray[0]);
                     access.DeleteRole(new RoleModel(id));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btn_add_permission_click(object sender, RoutedEventArgs e)
        {
            AddPermission window = new AddPermission(this);
            window.Show();
        }

        private void btn_delete_permission_click(object sender, RoutedEventArgs e)
        {
            if (PermissionGrid.SelectedItem == null)
            {
                MessageBox.Show("需选中数据表中的数据项");
            }
            else
            {
                try
                {
                    int id = Convert.ToInt32(((DataRowView)PermissionGrid.SelectedItem).Row.ItemArray[0]);
                    access.DeletePermission(new PermissionModel(id));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        #endregion

        #region tab2
        private void tab2_load(object sender, RoutedEventArgs e)
        {
            update_ura_table();
            update_pra_table();
            update_rra_table();
        }

        private void btn_add_ura_click(object sender, RoutedEventArgs e)
        {
            AddUserRole ura = new AddUserRole(this);
            ura.Show();
            ura.Closed += update_ura_table;
        }

        private void btn_delete_ura_click(object sender, RoutedEventArgs e)
        {
            if (UserRoleGrid.SelectedItem == null)
            {
                MessageBox.Show("需选中数据表中的数据项");
            }
            else
            {
                try
                {
                    int uid = Convert.ToInt32(((UserRoleModel)UserRoleGrid.SelectedItem).uid);
                    int rid = Convert.ToInt32(((UserRoleModel)UserRoleGrid.SelectedItem).rid);
                    access.RevokeUserRole(
                        new UserModel(uid),
                        new RoleModel(rid)
                        );
                    update_ura_table();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btn_add_rra_click(object sender, RoutedEventArgs e)
        {
            AddRoleRelation rra = new AddRoleRelation(this);
            rra.Show();
            rra.Closed += update_rra_table;
        }

        private void btn_delete_rra_click(object sender, RoutedEventArgs e)
        {
            if (RoleRoleGrid.SelectedItem == null)
            {
                MessageBox.Show("需选中数据表中的数据项");
            }
            else
            {
                try
                {
                    int parent = Convert.ToInt32(((RoleRelationModel)RoleRoleGrid.SelectedItem).parent);
                    int child = Convert.ToInt32(((RoleRelationModel)RoleRoleGrid.SelectedItem).child);
                    access.RevokeRoleChild(
                        new RoleModel(parent),
                        new RoleModel(child)
                        );
                    update_rra_table();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btn_add_pra_click(object sender, RoutedEventArgs e)
        {
            AddPermissionRole pra = new AddPermissionRole(this);
            pra.Show();
            pra.Closed += update_pra_table;
        }

        private void btn_delete_pra_click(object sender, RoutedEventArgs e)
        {
            if (PermissionRoleGrid.SelectedItem == null)
            {
                MessageBox.Show("需选中数据表中的数据项");
            }
            else
            {
                try
                {
                    int pid = Convert.ToInt32(((PermissionRoleModel)PermissionRoleGrid.SelectedItem).pid);
                    int rid = Convert.ToInt32(((PermissionRoleModel)PermissionRoleGrid.SelectedItem).rid);
                    access.RevokeRolePermission(
                        new RoleModel(rid),
                        new PermissionModel(pid)
                        );
                    update_pra_table();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        #region 用于刷新LINQ查询的结果表
        private void update_ura_table()
        {
            UserRoleGrid.ItemsSource = access.getURAView();
        }
        private void update_ura_table(object sender, EventArgs e)
        {
            UserRoleGrid.ItemsSource = access.getURAView();
        }
        private void update_pra_table()
        {
            PermissionRoleGrid.ItemsSource = access.getPRAView();
        }
        private void update_pra_table(object sender, EventArgs e)
        {
            PermissionRoleGrid.ItemsSource = access.getPRAView();
        }
        private void update_rra_table()
        {
            RoleRoleGrid.ItemsSource = access.getRRAView();
        }
        private void update_rra_table(object sender, EventArgs e)
        {
            RoleRoleGrid.ItemsSource = access.getRRAView();
        }
        #endregion

        #endregion

        #region tab3
        private void tab3_load(object sender, RoutedEventArgs e)
        {
            TreeViewModel s = new TreeViewModel(access);
            TreeView.DataContext = s;
        }

        private void treeview_selection_changed(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            int rid = Convert.ToInt32(((TreeNode)TreeView.SelectedItem).Id);
            RoleUsersGrid.ItemsSource = access.getRoleUsers(rid);
            RolePermissionsGrid.ItemsSource = access.getRolePermissions(rid);
        }
        #endregion

        #region tab4
        private void tab4_load(object sender, RoutedEventArgs e)
        {
            access.GetExclusionList();
        }

        private void btn_add_exclusion_click(object sender, RoutedEventArgs e)
        {
            AddExclusion window = new AddExclusion(this);
            window.Show();
        }

        private void btn_delete_exclusion_click(object sender, RoutedEventArgs e)
        {
            if (ExclusionGrid.SelectedItem == null)
            {
                MessageBox.Show("需选中数据表中的数据项");
            }
            else
            {
                try
                {
                    int id = Convert.ToInt32(((DataRowView)ExclusionGrid.SelectedItem).Row.ItemArray[0]);
                    access.DeleteExclusion(
                        new ExclusionModel(id)
                        );
                    update_rra_table();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        #endregion

        private void btn_logout_click(object sender, RoutedEventArgs e)
        {
            Login window = new Login();
            window.Show();
            this.Close();
        }

    }
}
