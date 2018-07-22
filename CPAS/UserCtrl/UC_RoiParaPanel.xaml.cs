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

namespace CPAS.UserCtrl
{
    /// <summary>
    /// UC_RoiParaPanel.xaml 的交互逻辑
    /// </summary>
    public partial class UC_RoiParaPanel : UserControl
    {
        public UC_RoiParaPanel()
        {
            InitializeComponent();
        }
        public int CurCamID{get;set;}
        public DependencyProperty CurCamIDProperty = DependencyProperty.Register("CurCamID", typeof(int), typeof(UC_RoiParaPanel));

        private void BtnSaveRoiPara_Click(object sender, RoutedEventArgs e)
        {
            //var VM = DataContext as MainWindowViewModel;
            //VM.SaveModelParaCommand.Execute($"Roi&{CurCamID}");
        }

        private void BtnTestRoi_Click(object sender, RoutedEventArgs e)
        {
            var VM = DataContext as MainWindowViewModel;
            VM.TestRoiModelCommand.Execute($"Roi&{CurCamID}");
        }

    }
}
