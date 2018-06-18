using CPAS.Classes;
using CPAS.UserCtrl;
using CPAS.Vision;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Models
{
    public class RoiItem
    {
        public string StrName { get; set; }
        public string StrFullName { get; set; }
        public RelayCommand<RoiItem> OperateAdd
        {
            get
            {
                return new RelayCommand<RoiItem>(item =>
                {
                    Vision.Vision.Instance.DrawRoi(Convert.ToInt16(item.StrFullName.Substring(3, 1)));
                });
            }
        }
        public RelayCommand<RoiItem> OperateEdit
        {
            get
            {
                return new RelayCommand<RoiItem>(item =>
                {
                    Console.WriteLine(item.StrName);
                });
            }
        }
        public RelayCommand<RoiItem> OperateDelete => new RelayCommand<RoiItem>(item =>
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(FileHelper.GetCurFilePathString());
            sb.Append("VisionData\\Roi\\");
            sb.Append(item.StrFullName);
            sb.Append(".reg");
            if (UC_MessageBox.Instance.ShowBox(string.Format("确定要删除{0}吗?", item.StrName)) == System.Windows.MessageBoxResult.Yes)
            {
                FileHelper.DeleteFile(sb.ToString());
                Messenger.Default.Send<string>(item.StrFullName, "UpdateRoiFiles");
            }
        });

    }
}
