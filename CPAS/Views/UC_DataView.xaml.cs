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
using GalaSoft.MvvmLight.Messaging;
using NUnit.Framework;

namespace CPAS.Views
{
    /// <summary>
    /// UC_DataView.xaml 的交互逻辑
    /// </summary>
    public partial class UC_DataView : UserControl , Iauthor
    {
        private Sqlite3Helper sql=new Sqlite3Helper("data source = LogData/data.db");
        private SQLiteDataReader reader = null;
        private string strTableName = "NGLogTable";
        public UC_DataView()
        {
            InitializeComponent();
            LogDataCollect = new ObservableCollection<LogDataItem>();
            Messenger.Default.Register<Tuple<string,string>>(this, "OperateDatabase", tuple => {
                string cmd = tuple.Item1;
                string para = tuple.Item2;
                switch (cmd)
                {
                    case "Delete":
                        switch (para)
                        {
                            case "OneWeek":
                                sql.DeleteValuesOR1(strTableName, new string[] { @"date('now', '-7 day')" }, new string[] { @"date(Time)" }, new string[] { ">=" });
                                break;
                            case "HalfMonth":
                                sql.DeleteValuesOR1(strTableName, new string[] { @"date('now', '-15 day')" }, new string[] { @"date(Time)" }, new string[] { ">=" });
                                break;
                            case "OneMonth":
                                sql.DeleteValuesOR1(strTableName, new string[] { @"date('now', '-30 day')" }, new string[] { @"date(Time)" }, new string[] { ">=" });
                                break;
                        }
                        break;

                    case "Add":
                        //string[] substring=para.Substring('&');
                        break; ;
                    default:
                        break;
                }
            });
            
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

    
        public void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            //Messenger.Default.Send<Tuple<string, string>>(new Tuple<string, string>("Delete", "OneWeek"), "OperateDatabase");

            if (TextBoxBarcode.Text.Trim() == "")
                return;
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
