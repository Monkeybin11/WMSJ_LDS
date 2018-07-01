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
        private string _curSelectedPrescription = "";
        private EnumBadBarcodeExpiration _badBarcodeExpiration;
        public string CurSelectedPrescription {
            get { return _curSelectedPrescription; }
            set {
                SetCurSelectedPrescription(value);
            }
        }
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
        public void SetCurSelectedPrescription(string strName)
        {
            if (strName != _curSelectedPrescription)
            {
                _curSelectedPrescription = strName;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurSelectedPrescription"));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        
    }
}
