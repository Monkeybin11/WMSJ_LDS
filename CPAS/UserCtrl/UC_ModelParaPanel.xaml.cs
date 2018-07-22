using CPAS.Models;
using CPAS.ViewModels;
using GalaSoft.MvvmLight.Command;
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
        public int CurCamID
        {
            get { return Convert.ToInt16(GetValue(CurCamIDProperty).ToString()); }
            set { SetValue(CurCamIDProperty, value); }
        }
        public DependencyProperty CurCamIDProperty = DependencyProperty.Register("CurCamID", typeof(int), typeof(UC_ModelParaPanel));

 
        public RoiModelBase CurSelectItemOfListBox
        {
            get { return  GetValue(CurSelectItemOfListBoxProperty) as RoiModelBase; }
            set { SetValue(CurSelectItemOfListBoxProperty, value); }
        }
        public DependencyProperty CurSelectItemOfListBoxProperty = DependencyProperty.Register("CurSelectItemOfListBox", typeof(RoiModelBase), typeof(UC_ModelParaPanel));



        private void BtnSaveModelPara_Click(object sender, RoutedEventArgs e)
        {
            var VM = DataContext as MainWindowViewModel;
            VM.SaveModelParaCommand.Execute(new Tuple<RoiModelBase, int>(CurSelectItemOfListBox, CurCamID));
        }

        private void BtnTestModel_Click(object sender, RoutedEventArgs e)
        {
            var VM = DataContext as MainWindowViewModel;
            VM.TestModelParaCommand.Execute(new Tuple<RoiModelBase, int>(CurSelectItemOfListBox, CurCamID));
        }

        private void MaxThreSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            (DataContext as MainWindowViewModel).PreViewRoiCommand.Execute(new Tuple<RoiModelBase,int>(CurSelectItemOfListBox, CurCamID));
        }

        private void MinThreSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            (DataContext as MainWindowViewModel).PreViewRoiCommand.Execute(new Tuple<RoiModelBase, int>(CurSelectItemOfListBox, CurCamID));
        }
    }
}
