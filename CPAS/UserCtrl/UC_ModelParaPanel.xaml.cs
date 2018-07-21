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
    /// UC_ModelParaPanel.xaml 的交互逻辑
    /// </summary>
    public partial class UC_ModelParaPanel : UserControl
    {
        public UC_ModelParaPanel()
        {
            InitializeComponent();
        }
        public int CurCamID { get; set; }
        public DependencyProperty CurCamIDProperty = DependencyProperty.Register("CurCamID", typeof(int), typeof(UC_ModelParaPanel));

        private void BtnSaveModelPara_Click(object sender, RoutedEventArgs e)
        {
            var VM = DataContext as MainWindowViewModel;
            VM.SaveRoiModelParaCommand.Execute($"Model&{CurCamID}");
        }

        private void BtnTestModel_Click(object sender, RoutedEventArgs e)
        {
            var VM = DataContext as MainWindowViewModel;
            VM.SaveRoiModelParaCommand.Execute($"Model&{CurCamID}");
        }

        private void MaxThreSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Vision.Vision.Instance.PreCreateShapeModel(CurCamID, Convert.ToInt16(MinThreSlider.Value), Convert.ToInt16(MaxThreSlider.Value), Vision.EnumShapeModelType.XLD);
        }

        private void MinThreSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Vision.Vision.Instance.PreCreateShapeModel(CurCamID, Convert.ToInt16(MinThreSlider.Value), Convert.ToInt16(MaxThreSlider.Value), Vision.EnumShapeModelType.XLD);
        }
    }
}
