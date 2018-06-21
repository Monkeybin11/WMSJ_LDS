using CPAS.ViewModels;
using CPAS.Vision;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
namespace CPAS.Converters
{
    public class EnumCamSnapState2SnapButtonEnable : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bRet = false;
            EnumCamSnapState CamSnapState = (EnumCamSnapState)value;
            string strPata = parameter.ToString();
            switch (strPata)
            {
                case "SnapOnce":
                    bRet = CamSnapState == EnumCamSnapState.IDLE;
                    break;
                case "SnapContinues":
                    bRet = CamSnapState == EnumCamSnapState.IDLE;
                    break;
                case "StopSnap":
                    bRet= CamSnapState == EnumCamSnapState.BUSY;
                    break;
                case "ListBoxForRoiAndTemplate":
                    bRet = CamSnapState == EnumCamSnapState.IDLE;
                    break;
                default:
                    throw new Exception("Unknow cmd for converter named EnumCamSnapState2SnapButtonEnable");
            }
            return bRet;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
