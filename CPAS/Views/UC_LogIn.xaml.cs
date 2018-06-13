using CPAS.Interface;
using CPAS.ViewModels;
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

namespace CPAS.Views
{
    /// <summary>
    /// UC_LogIn.xaml 的交互逻辑
    /// </summary>
    public partial class UC_LogIn : UserControl 
    {
        private MainWindowViewModel vm;
       

        public UC_LogIn()
        {
            InitializeComponent();

        }

   
        private void BtnLogOut_Click(object sender, RoutedEventArgs e)
        {
            vm.LogOutCommand.Execute(null);
            UsrTextBox.Text = "";
            PsdTextBox.Password = "";
        }

        private void BtnLogIn_Click(object sender, RoutedEventArgs e)
        {
            vm.LogInCommand.Execute(LogPara);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            vm = DataContext as MainWindowViewModel;
        }

      

        public Tuple<string, string> LogPara { get { return new Tuple<string, string>(UsrTextBox.Text, PsdTextBox.Password); } set { } }

   
    }
}
