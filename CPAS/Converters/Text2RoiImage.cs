using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CPAS.Converters
{
    class Text2RoiImage : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strCameraState = value.ToString().ToUpper();
            BitmapImage bitmap = null;
            switch (strCameraState)
            {
                case "NEW":
                    bitmap = new BitmapImage(new Uri(@"..\Images\New.png", UriKind.Relative));
                    break;
                case "EDIT":
                    bitmap = new BitmapImage(new Uri(@"..\Images\Edit.png", UriKind.Relative));
                    break;
                case "DELETE":
                    bitmap = new BitmapImage(new Uri(@"..\Images\Delete.png", UriKind.Relative));
                    break;
                default:
                    break;
            }
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
