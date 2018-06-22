using CPAS.Config;
using CPAS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CPAS.Converters
{
    public class Int2PrescriptionModel : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int nCurSel = (int)value;
            if (nCurSel >= 0)
            {
                if (ConfigMgr.PrescriptionCfgMgr.Prescriptions.Count() > nCurSel)
                    return ConfigMgr.PrescriptionCfgMgr.Prescriptions[nCurSel];
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
