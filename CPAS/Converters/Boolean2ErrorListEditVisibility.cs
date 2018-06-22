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
    class Boolean2ErrorListEditVisibility : IValueConverter
    {
  
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (parameter.ToString())
            {
                case "PLC":
                    return (bool)value ? Visibility.Visible : Visibility.Hidden;
                case "Sys":
                    return (bool)value ? Visibility.Hidden : Visibility.Visible;
                default:
                    return false;
            }
        }

   
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
 
}
