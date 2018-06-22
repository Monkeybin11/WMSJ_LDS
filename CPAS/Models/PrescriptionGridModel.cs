using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Models
{
    public class PrescriptionGridModel : INotifyPropertyChanged
    {
        private string _name;
        private string _remark;
        private bool _record;
        private bool _unLock;
        private bool _turnLaser;
        private bool _tune1;
        private bool _tune2;
        private bool _clib;


        public PrescriptionGridModel()
        {
            Name = "配方1";
            Remark = "备注1";
            Record = true;
            UnLock = true;
            TuneLaser = true;
            Tune1 = true;
            Tune2 = true;
            Calib = true;
        }

        [CategoryAttribute("配方"), DescriptionAttribute("Set the file path of log")]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
                }
            }
        }
        [CategoryAttribute("配方"), DescriptionAttribute("Set the file path of log")]
        public string Remark
        {
            get { return _remark; }
            set
            {
                if (_remark != value)
                {
                    _remark = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Remark"));
                }
            }
        }
        [CategoryAttribute("配方"), DescriptionAttribute("Set the file path of log")]
        public bool Record
        {
            get { return _record; }
            set
            {
                if (_record != value)
                {
                    _record = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Record"));
                }
            }
        }
        [CategoryAttribute("配方"), DescriptionAttribute("Set the file path of log")]
        public bool UnLock
        {
            get { return _unLock; }
            set
            {
                if (_unLock != value)
                {
                    _unLock = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Unlock"));
                }
            }
        }
        [CategoryAttribute("配方"), DescriptionAttribute("Set the file path of log")]
        public bool TuneLaser
        {
            get { return _turnLaser; }
            set
            {
                if (_turnLaser != value)
                {
                    _turnLaser = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TuneLaser"));
                }
            }
        }
        [CategoryAttribute("配方"), DescriptionAttribute("Set the file path of log")]
        public bool Tune1
        {
            get { return _tune1; }
            set
            {
                if (_tune1 != value)
                {
                    _tune1 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tune1"));
                }
            }
        }
        [CategoryAttribute("配方"), DescriptionAttribute("Set the file path of log")]
        public bool Tune2
        {
            get { return _tune2; }
            set
            {
                if (_tune2 != value)
                {
                    _tune2 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tune2"));
                }
            }
        }
        [CategoryAttribute("配方"), DescriptionAttribute("Set the file path of log")]
        public bool Calib
        {
            get { return _clib; }
            set
            {
                if (_clib != value)
                {
                    _clib = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Calib"));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
