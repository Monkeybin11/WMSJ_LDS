using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CPAS.Converters
{
    public class Int2Visibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility bRet=Visibility.Hidden;
            switch (parameter.ToString())
            {
                case "Roi":
                    bRet=(int)value == 1? Visibility.Visible : Visibility.Hidden;
                    break;
                case "Model":
                    bRet=(int)value == 0 ? Visibility.Visible : Visibility.Hidden;
                    break;
            }
            return bRet;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
