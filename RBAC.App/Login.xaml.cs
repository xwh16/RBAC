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

namespace RBAC.App
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : MetroWindow
    {
        // 管理员登录口令
        private string AdminToken = "admin";

        public Login()
        {
            InitializeComponent();

#if DEBUG
            // check_admin("admin");
#endif

            //Set hot key Enter
            RoutedCommand EnterCmd = new RoutedCommand();
            EnterCmd.InputGestures.Add(new KeyGesture(Key.Enter));
            CommandBindings.Add(new CommandBinding(EnterCmd, btn_login_click));
        }

        private void btn_login_click(object sender, RoutedEventArgs e)
        {
            try
            {
                UserModel user = new UserModel(0, username.Text, password.Password);
                RoleBasedAccessControlSystem.Login(user);
                User.UserWindow window = new User.UserWindow();
                MessageBox.Show("登录成功");
                window.Show();
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_admin_click(object sender, RoutedEventArgs e)
        {
            QuickInput input = new QuickInput("管理员凭据");
            input.SubmitEvent += check_admin;
            input.Show();
        }

        private void check_admin(string token)
        {
            if (token == AdminToken)
            {
                MessageBox.Show("管理员登录成功");
                // show administrator panel
                Admin.AdminWindow window = new Admin.AdminWindow();
                window.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("登录失败");
            }
        }
    }
}
