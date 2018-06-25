using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Models
{
    public class SystemParaModel
    {
        public enum BARCODESOURCE { FILE, SCANNER }
  
        private BARCODESOURCE _barcodeSource;
        private int _barcodeLength;
        private double _ldsPower = 0.3;


        [CategoryAttribute("条码设置"), DescriptionAttribute("设置条码来源;FILE—》从*.xls文件读取，SCANNER—》扫码枪扫取")]
        public BARCODESOURCE BarcodeSource
        {
            get { return _barcodeSource; }
            set { _barcodeSource = value; }
        }
        [CategoryAttribute("条码设置"), DescriptionAttribute("设置条码长度")]
        public int BarcodeLength
        {
            get { return _barcodeLength; }
            set { _barcodeLength = value; }
        }

        [CategoryAttribute("LDS测试功率设置"), DescriptionAttribute("设置LDS的功率窗口")]
        public double LDSPower
        {
            get { return _ldsPower; }
            set { _ldsPower = value; }
        }

 
    }
}
