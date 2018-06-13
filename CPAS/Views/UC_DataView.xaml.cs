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
using CPAS.Interface;

namespace CPAS.Views
{
    /// <summary>
    /// UC_DataView.xaml 的交互逻辑
    /// </summary>
    public partial class UC_DataView : UserControl , Iauthor
    {
        public UC_DataView()
        {
            InitializeComponent();
            Sqlite3Helper sqlite = new Sqlite3Helper("../LogData/data.db");
        }

        public int Level { get; set; }
        public static DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(int), typeof(UC_DataView));
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
