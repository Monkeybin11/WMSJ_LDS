using CPAS.UserCtrl;
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

namespace CPAS.Views
{
    /// <summary>
    /// Window_AddNewPrescription.xaml 的交互逻辑
    /// </summary>
    public partial class Window_AddNewPrescription : Window
    {
        public Window_AddNewPrescription()
        {
            InitializeComponent();
        }
        private static MessageBoxResult _msgresult = MessageBoxResult.No;
        public static Tuple<string, string> ProfileValue = null;  
        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            _msgresult = MessageBoxResult.Yes;
            if (EditBoxName.Text == "")
                UC_MessageBox.ShowMsgBox("配方名称不能为空", "错误");
            else
            {
                ProfileValue = new Tuple<string, string>(EditBoxName.Text, EditBoxRemark.Text);
                Close();
            }
        }
        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            _msgresult = MessageBoxResult.No;
            ProfileValue = new Tuple<string, string>("", "");
            Close();
        }
        public static MessageBoxResult ShowWindowNewDescription()
        {
            Window_AddNewPrescription dlg = new Window_AddNewPrescription();
            dlg.ShowDialog();
            return _msgresult;
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
