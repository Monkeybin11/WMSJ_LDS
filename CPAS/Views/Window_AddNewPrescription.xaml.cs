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
        ~Window_AddNewPrescription()
        {
            _instance = null;
        }
        private static Window_AddNewPrescription _instance = null;
        private MessageBoxResult _msgresult = MessageBoxResult.No;
        public Tuple<string, string> ProfileValue = null;
        private bool bClose = false;

        public static Window_AddNewPrescription Instance{
            get
            {
                if(_instance==null)
                    _instance= new Window_AddNewPrescription();
                return _instance;
            }
        }
        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            _msgresult = MessageBoxResult.Yes;
            ProfileValue= new Tuple<string, string>(EditBoxName.Text, EditBoxRemark.Text);
            Hide();
        }
        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            _msgresult = MessageBoxResult.No;
            ProfileValue = null;
            Hide();
        }
        public MessageBoxResult ShowWindowNewDescription()
        {
            EditBoxName.Text = "";
            EditBoxRemark.Text = "";
            ShowDialog();
            return _msgresult;
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !bClose;
        }
        public void SetCloseFlag(bool bClose)
        {
            this.bClose = bClose;
            Close();
        }
    }
}
