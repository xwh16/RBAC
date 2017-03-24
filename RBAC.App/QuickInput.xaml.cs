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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace RBAC.App
{
    /// <summary>
    /// QuickInput.xaml 的交互逻辑
    /// </summary>
    public partial class QuickInput : MetroWindow
    {
        public delegate void Submit(string text);

        public event Submit SubmitEvent;

        public QuickInput(string lab)
        {
            InitializeComponent();
            label.Content = lab;

            //Set hot key Enter
            RoutedCommand EnterCmd = new RoutedCommand();
            EnterCmd.InputGestures.Add(new KeyGesture(Key.Enter));
            CommandBindings.Add(new CommandBinding(EnterCmd, button_Click));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            SubmitEvent(textBox.Text);
            this.Close();
        }
    }
}
