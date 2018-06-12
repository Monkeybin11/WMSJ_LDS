using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace CPAS.Converters
{
    public class MsgType2ForeBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CPAS.Models.MSGTYPE msg = (CPAS.Models.MSGTYPE)value;
            Brush brush = null;
            switch (msg)
            {
                case Models.MSGTYPE.INFO:
                    brush =new SolidColorBrush(Color.FromRgb(0,0,0));
                    break;
                case Models.MSGTYPE.WARNING:
                    brush = new SolidColorBrush(Color.FromRgb(200,200,0));
                    break;
                case Models.MSGTYPE.ERROR:
                    brush = new SolidColorBrush(Color.FromRgb(255,0,0));
                    break;
                default:
                    break;
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
