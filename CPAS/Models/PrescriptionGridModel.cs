using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Models
{
    public class PrescriptionGridModel : INotifyPropertyChanged, ICloneable
    {
        private string _name;
        private string _remark;
        private bool _unLock;   //解锁
        private bool _readBarcode;
        private bool _adjustLaser;
        private bool _adjustHoriz;
        private bool _adjustFocus;
        private bool _calibration;


        //public PrescriptionGridModel()
        //{
        //    Name = "配方1";
        //    Remark = "备注1";
        //    Record = true;
        //    UnLock = true;
        //    TuneLaser = true;
        //    Tune1 = true;
        //    Tune2 = true;
        //    Calib = true;
        //}

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
        public bool UnLock
        {
            get { return _unLock; }
            set
            {
                if (_unLock != value)
                {
                    _unLock = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UnLock"));
                }
            }
        }
        [CategoryAttribute("配方"), DescriptionAttribute("Set the file path of log")]
        public bool ReadBarcode
        {
            get { return _readBarcode; }
            set
            {
                if (_readBarcode != value)
                {
                    _readBarcode = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReadBarcode"));
                }
            }
        }
        [CategoryAttribute("配方"), DescriptionAttribute("Set the file path of log")]
        public bool AdjustLaser
        {
            get { return _adjustLaser; }
            set
            {
                if (_adjustLaser != value)
                {
                    _adjustLaser = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AdjustLaser"));
                }
            }
        }
        [CategoryAttribute("配方"), DescriptionAttribute("Set the file path of log")]
        public bool AdjustHoriz
        {
            get { return _adjustHoriz; }
            set
            {
                if (_adjustHoriz != value)
                {
                    _adjustHoriz = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AdjustHoriz"));
                }
            }
        }
        [CategoryAttribute("配方"), DescriptionAttribute("Set the file path of log")]
        public bool AdjustFocus
        {
            get { return _adjustFocus; }
            set
            {
                if (_adjustFocus != value)
                {
                    _adjustFocus = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AdjustFocus"));
                }
            }
        }
        [CategoryAttribute("配方"), DescriptionAttribute("Set the file path of log")]
        public bool Calibration
        {
            get { return _calibration; }
            set
            {
                if (_calibration != value)
                {
                    _calibration = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Calibration"));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public object Clone()
        {
            return new PrescriptionGridModel()
            {
                Name = this.Name,
                Remark = this.Remark,
                UnLock = this.UnLock,
                ReadBarcode = this.ReadBarcode,
                AdjustLaser = this.AdjustLaser,
                AdjustHoriz = this.AdjustHoriz,
                AdjustFocus = this.AdjustFocus,
                Calibration = this.Calibration
            };
        }
    }
}
