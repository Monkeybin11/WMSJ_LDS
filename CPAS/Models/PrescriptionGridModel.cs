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
        //工序设置
        private string _name;
        private string _remark;
        private bool _unLock;   //解锁
        private bool _readBarcode;
        private bool _adjustLaser;
        private bool _adjustHoriz;
        private bool _adjustFocus;
        private bool _calibration;

        //LDS设置
        public enum BARCODESOURCE { FILE, SCANNER }
        private BARCODESOURCE _barcodeSource;
        private int _barcodeLength;
        private double[] _ldsPower = new double[] { 105, 107 };
        private double[] _ldsHoriValue2m = new double[] { 400, 730 };
        private double _ldsHoriValue6m = 170;
        private Int32 _cMosPointNumber = 1536;


        [CategoryAttribute("工序配方"), DescriptionAttribute("工序名称")]
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
        [CategoryAttribute("工序配方"), DescriptionAttribute("备注")]
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
        [CategoryAttribute("工序配方"), DescriptionAttribute("是否启用解锁")]
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
        [CategoryAttribute("工序配方"), DescriptionAttribute("是否启用扫码")]
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
        [CategoryAttribute("工序配方"), DescriptionAttribute("是否启用调整激光功率")]
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
        [CategoryAttribute("工序配方"), DescriptionAttribute("是否启用调整水平")]
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
        [CategoryAttribute("工序配方"), DescriptionAttribute("是否启用调焦距")]
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
        [CategoryAttribute("工序配方"), DescriptionAttribute("是否启用距离标定")]
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



        [CategoryAttribute("条码设置"), DescriptionAttribute("设置条码来源;FILE—》从*.xls文件读取，SCANNER—》扫码枪扫取")]
        public BARCODESOURCE BarcodeSource
        {
            get { return _barcodeSource; }
            set
            {
                if (_barcodeSource != value)
                {
                    _barcodeSource = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BarcodeSource"));
                }
            }
        }
        [CategoryAttribute("条码设置"), DescriptionAttribute("设置条码长度")]
        public int BarcodeLength
        {
            get { return _barcodeLength; }
            set
            {
                if (_barcodeLength != value)
                {
                    _barcodeLength = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BarcodeLength"));
                }
            }
        }



        [CategoryAttribute("LDS设置"), DescriptionAttribute("设置LDS的功率")]
        public double[] LDSPower
        {
            get { return _ldsPower; }
            set
            {
                if (_ldsPower != value)
                {
                    _ldsPower = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LDSPower"));
                }
            }
        }
        [CategoryAttribute("LDS设置"), DescriptionAttribute("设置LDS在2米处的激光强度值")]
        public double[] LDSHoriValue2m
        {
            get { return _ldsHoriValue2m; }
            set
            {
                if (_ldsHoriValue2m != value)
                {
                    _ldsHoriValue2m = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LDSHoriValue2m"));
                }
            }
        }
        [CategoryAttribute("LDS设置"), DescriptionAttribute("设置LDS在6米处的激光强度值")]
        public double LDSHoriValue6m
        {
            get { return _ldsHoriValue6m; }
            set
            {
                if (_ldsHoriValue6m != value)
                {
                    _ldsHoriValue6m = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LDSHoriValue6m"));
                }
            }
        }
        [CategoryAttribute("LDS设置"), DescriptionAttribute("设置LDS的CMOS相机点数")]
        public Int32 CMosPointNumber
        {
            get { return _cMosPointNumber; }
            set
            {
                if (_cMosPointNumber != value)
                {
                    _cMosPointNumber = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CMosPointNumber"));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
   
    }
}
