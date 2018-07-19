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
    public class Int2ContentControl : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataTemplate t = null;
            if (value == null)
                return null;
            else
            {
                if((int)value==1)
                    t= (DataTemplate)Application.Current.TryFindResource("RoiPanel");
                else
                    t= (DataTemplate)Application.Current.TryFindResource("ModelPanel");
                return t;
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
