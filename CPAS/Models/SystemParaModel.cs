using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Models
{
    public class SystemParaModel : INotifyPropertyChanged
    {
        private string _currentPrescriptionUsed;
        public string CurrentPrescriptionUsed
        {
            get{ return _currentPrescriptionUsed; }
            set
            {
                if (_currentPrescriptionUsed != value)
                {
                    _currentPrescriptionUsed = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentPrescriptionUsed"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
