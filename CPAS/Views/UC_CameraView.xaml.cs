using CPAS.Interface;
using System;
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

namespace CPAS.Views
{
    /// <summary>
    /// UC_CameraView.xaml 的交互逻辑
    /// </summary>
    public partial class UC_CameraView : UserControl , Iauthor
    {
        public UC_CameraView()
        {
            InitializeComponent();
        }

        public int Level { get; set; }
        public static DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(int), typeof(UC_CameraView));
        public int GetLever()
        {
            throw new NotImplementedException();
        }

        public void SetLever(int nLever)
        {
            throw new NotImplementedException();
        }
    }
}
