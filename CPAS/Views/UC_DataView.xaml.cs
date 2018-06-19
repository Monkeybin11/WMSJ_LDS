using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
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
using CPAS.Models;

namespace CPAS.Views
{
    /// <summary>
    /// UC_DataView.xaml 的交互逻辑
    /// </summary>
    public partial class UC_DataView : UserControl , Iauthor
    {
        private Sqlite3Helper sql=new Sqlite3Helper("data source = LogData/data.db");
        private SQLiteDataReader reader = null;
        
        public UC_DataView()
        {
            InitializeComponent();
            LogDataCollect = new ObservableCollection<LogDataItem>();
            
        }

        public ObservableCollection<LogDataItem> LogDataCollect { get; set; }
        public Visibility IsHitTextVisable { get { return (Visibility)GetValue(IsHitTextVisableProperty); } set { SetValue(IsHitTextVisableProperty, value); } }
        public static DependencyProperty IsHitTextVisableProperty = DependencyProperty.Register("IsHitTextVisable", typeof(Visibility), typeof(UC_DataView));
        public int Level { get { return (int)GetValue(LevelProperty); } set { SetValue(LevelProperty, value); } }
        public static DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(int), typeof(UC_DataView));
        public int GetLever()
        {
            throw new NotImplementedException();
        }

        public void SetLever(int nLever)
        {
            throw new NotImplementedException();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LogDataCollect.Clear();
            string strSql = "";
            if ((bool)CbLike.IsChecked)
                strSql = string.Format(@"select * from NGLogTable where SN like '%{0}%'",  TextBoxBarcode.Text);
            else
                strSql = string.Format(@"select * from NGLogTable where SN='{0}'", TextBoxBarcode.Text);
            reader =sql.ExecuteQuery(strSql);
            while (reader.Read())
            {
                LogDataCollect.Add(new LogDataItem()
                {
                    SN = reader.GetString(reader.GetOrdinal("SN")),
                    Time = reader.GetDateTime(reader.GetOrdinal("Time")).GetDateTimeFormats()[35],
                    Result = reader.GetString(reader.GetOrdinal("Result")),
                    Reason = reader.GetString(reader.GetOrdinal("Reason"))
                });
            } 
        }

        private void TextBoxBarcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsHitTextVisable = TextBoxBarcode.Text == "" ? Visibility.Visible : Visibility.Hidden;
        }

    }
    
}
