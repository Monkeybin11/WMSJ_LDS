using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Models
{
    public enum EnumBadBarcodeExpiration
    {
        OneWeek,
        HalfMonth,
        OneMonth
    }
    public class SystemParaModel : INotifyPropertyChanged
    {
        
        private EnumBadBarcodeExpiration _badBarcodeExpiration;
        public EnumBadBarcodeExpiration BadBarcodeExpiration
        {
            get { return _badBarcodeExpiration; }
            set
            {
                if (_badBarcodeExpiration != value)
                {
                    _badBarcodeExpiration = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BadBarcodeExpiration"));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
       
    }
}
