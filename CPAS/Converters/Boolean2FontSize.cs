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
    class Boolean2FontSize : IValueConverter
    {
  
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (parameter.ToString())
            {
                case "PLC":
                    return (bool)value ? 20.0f : 15.0f;
                case "Sys":
                    return (bool)value ? 15.0f : 20.0f;
                default:
                    return 15.0f;
            }
        }

   
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
 
}
