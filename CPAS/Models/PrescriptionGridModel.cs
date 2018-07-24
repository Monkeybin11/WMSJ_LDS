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
        private UInt16[] _ldsHoriValue2m = new UInt16[] { 400, 730 };
        private UInt16 _ldsHoriValue6m = 170;
        private Int32 _cMosPointNumber = 1536;



        //视觉部分设置
        //ROI设置
        private string _roiCam1 = "";
        private string _roiCam2 = "";
        private string _roiCam3 = "";
        private string _roiCam4 = "";
        private string _roiCam5 = "";
        private string _roiCam6 = "";


        //模板设置
        private string _modelCam1 = "";
        private string _modelCam2 = "";
        private string _modelCam3 = "";
        private string _modelCam4 = "";
        private string _modelCam5 = "";
        private string _modelCam6 = "";





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
        public bool EnableUnLock
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
        public bool EnableReadBarcode
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
        public bool EnableAdjustLaser
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
        public bool EnableAdjustHoriz
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
        public bool EnableAdjustFocus
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
        public bool EnableCalibration
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
        public UInt16[] LDSHoriValue2m
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
        public UInt16 LDSHoriValue6m
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


        [CategoryAttribute("相机Roi设置"), DescriptionAttribute("设置相机1的ROI")]
        [ReadOnly(true)]
        public string RoiCam1
        {
            get { return _roiCam1; }
            set
            {
                if (_roiCam1 != value)
                {
                    _roiCam1 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RoiCam1"));
                }
            }
        }
        [CategoryAttribute("相机Roi设置"), DescriptionAttribute("设置相机2的ROI")]
        [ReadOnly(true)]
        public string RoiCam2
        {
            get { return _roiCam2; }
            set
            {
                if (_roiCam2 != value)
                {
                    _roiCam2 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RoiCam2"));
                }
            }
        }
        [CategoryAttribute("相机Roi设置"), DescriptionAttribute("设置相机3的ROI")]
        [ReadOnly(true)]
        public string RoiCam3
        {
            get { return _roiCam3; }
            set
            {
                if (_roiCam3 != value)
                {
                    _roiCam3 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RoiCam3"));
                }
            }
        }

        [CategoryAttribute("相机Roi设置"), DescriptionAttribute("设置相机4的ROI")]
        [ReadOnly(true)]
        public string RoiCam4
        {
            get { return _roiCam4; }
            set
            {
                if (_roiCam4 != value)
                {
                    _roiCam4 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RoiCam4"));
                }
            }
        }

        [CategoryAttribute("相机Roi设置"), DescriptionAttribute("设置相机5的ROI")]
        [ReadOnly(true)]
        public string RoiCam5
        {
            get { return _roiCam5; }
            set
            {
                if (_roiCam5 != value)
                {
                    _roiCam5 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RoiCam5"));
                }
            }
        }

        [CategoryAttribute("相机Roi设置"), DescriptionAttribute("设置相机6的ROI")]
        [ReadOnly(true)]
        public string RoiCam6
        {
            get { return _roiCam6; }
            set
            {
                if (_roiCam6 != value)
                {
                    _roiCam6 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RoiCam6"));
                }
            }
        }




        [CategoryAttribute("相机模板设置"), DescriptionAttribute("设置相机1的模板")]
        [ReadOnly(true)]
        public string ModelCam1
        {
            get { return _modelCam1; }
            set
            {
                if (_modelCam1 != value)
                {
                    _modelCam1 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ModelCam1"));
                }
            }
        }
        [CategoryAttribute("相机模板设置"), DescriptionAttribute("设置相机2的模板")]
        [ReadOnly(true)]
        public string ModelCam2
        {
            get { return _modelCam2; }
            set
            {
                if (_modelCam2 != value)
                {
                    _modelCam2 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ModelCam2"));
                }
            }
        }
        [CategoryAttribute("相机模板设置"), DescriptionAttribute("设置相机3的模板")]
        [ReadOnly(true)]
        public string ModelCam3
        {
            get { return _modelCam3; }
            set
            {
                if (_modelCam3 != value)
                {
                    _modelCam3 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ModelCam3"));
                }
            }
        }

        [CategoryAttribute("相机模板设置"), DescriptionAttribute("设置相机4的模板")]
        [ReadOnly(true)]
        public string ModelCam4
        {
            get { return _modelCam4; }
            set
            {
                if (_modelCam4 != value)
                {
                    _modelCam4 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ModelCam4"));
                }
            }
        }

        [CategoryAttribute("相机模板设置"), DescriptionAttribute("设置相机5的模板")]
        [ReadOnly(true)]
        public string ModelCam5
        {
            get { return _modelCam5; }
            set
            {
                if (_modelCam5 != value)
                {
                    _modelCam5 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ModelCam5"));
                }
            }
        }

        [CategoryAttribute("相机模板设置"), DescriptionAttribute("设置相机6的模板")]
        [ReadOnly(true)]
        public string ModelCam6
        {
            get { return _modelCam6; }
            set
            {
                if (_modelCam6 != value)
                {
                    _modelCam6 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ModelCam6"));
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
                BarcodeLength = this.BarcodeLength,
                BarcodeSource = this.BarcodeSource,
                CMosPointNumber = this.CMosPointNumber,
                EnableAdjustFocus = this.EnableAdjustFocus,
                EnableAdjustHoriz = this.EnableAdjustHoriz,
                EnableAdjustLaser = this.EnableAdjustLaser,
                EnableCalibration = this.EnableCalibration,
                EnableReadBarcode = this.EnableReadBarcode,
                EnableUnLock = this.EnableUnLock,
                LDSHoriValue2m = new ushort[] { this.LDSHoriValue2m[0], this.LDSHoriValue2m[1] },
                LDSHoriValue6m = this.LDSHoriValue6m,
                LDSPower = new double[] { this.LDSPower[0], this.LDSPower[1] },
                ModelCam1 = this.ModelCam1,
                ModelCam2 = this.ModelCam2,
                ModelCam3 = this.ModelCam3,
                ModelCam4 = this.ModelCam4,
                ModelCam5 = this.ModelCam5,
                ModelCam6 = this.ModelCam6,
                RoiCam1 = this.RoiCam1,
                RoiCam2 = this.RoiCam2,
                RoiCam3 = this.RoiCam3,
                RoiCam4 = this.RoiCam4,
                RoiCam5 = this.RoiCam5,
                RoiCam6 = this.RoiCam6
            };
        }
    }
}
