using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace CPAS.Converters
{
    class Dictionary2StationHeaderBackagroundBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Dictionary<string, WorkFlow.WorkFlowBase> dic = value as Dictionary<string, WorkFlow.WorkFlowBase>;
            if (dic.Keys.Contains(parameter.ToString()))
                return new SolidColorBrush(Color.FromRgb(0,180,0));
            else
                return new SolidColorBrush(Color.FromRgb(128,128,128));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
