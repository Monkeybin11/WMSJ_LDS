using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// UC_StationHeader.xaml 的交互逻辑
    /// </summary>
    public partial class UC_StationHeader : UserControl
    {
        public UC_StationHeader()
        {
            InitializeComponent();
        }
        public string HeaderCaption { get { return GetValue(HeaderCaptionProperty).ToString(); } set { SetValue(HeaderCaptionProperty, value); } }
        public static DependencyProperty HeaderCaptionProperty = DependencyProperty.Register("HeaderCaption", typeof(string), typeof(UC_StationHeader));
        public Brush HeaderBackground { get { return GetValue(HeaderBackgroundProperty) as Brush; } set { SetValue(HeaderBackgroundProperty, value); } }
        public static DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register("HeaderBackground", typeof(Brush), typeof(UC_StationHeader));
        public ObservableCollection<string> StepInfoCollection { get { return GetValue(StepInfoCollectionProperty) as ObservableCollection<string>; } set { SetValue(StepInfoCollectionProperty, value); } }
        public static DependencyProperty StepInfoCollectionProperty = DependencyProperty.Register("StepInfoCollection", typeof(ObservableCollection<string>), typeof(UC_StationHeader));

    }
}
