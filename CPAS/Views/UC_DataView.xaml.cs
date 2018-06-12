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
using CPAS.Classes;
namespace CPAS.Views
{
    /// <summary>
    /// UC_DataView.xaml 的交互逻辑
    /// </summary>
    public partial class UC_DataView : UserControl
    {
        public UC_DataView()
        {
            InitializeComponent();
            Sqlite3Helper sqlite = new Sqlite3Helper("../LogData/data.db");
        }
    }
}
