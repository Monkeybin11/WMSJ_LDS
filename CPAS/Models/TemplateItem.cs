using CPAS.Classes;
using CPAS.UserCtrl;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Models
{
    public class TemplateItem : RoiModelBase
    {
        public override  RelayCommand<RoiModelBase> OperateAdd
        {
            get
            {
                return new RelayCommand<RoiModelBase>(item => {
                    var model = item as TemplateItem;
                    //Vision.Vision.Instance.CreateShapeModel(item.)
                    Console.WriteLine(model.StrName);
                });
            }
        }
        public override RelayCommand<RoiModelBase> OperateDelete
        {
            get
            {
                return new RelayCommand<RoiModelBase>(item => {
                    var model= item as TemplateItem;
                    StringBuilder sb = new StringBuilder();
                    sb.Append(FileHelper.GetCurFilePathString());
                    sb.Append("VisionData\\Model\\");
                    sb.Append(item.StrFullName);
                    sb.Append(".shm");
                    if (UC_MessageBox.ShowMsgBox(string.Format("确定要删除{0}吗?", item.StrName)) == System.Windows.MessageBoxResult.Yes)
                    {
                        FileHelper.DeleteFile(sb.ToString());
                        Messenger.Default.Send<int>(model.Index, "UpdateTemplateFiles");
                    }   
                });
            }
        }
    }
}
