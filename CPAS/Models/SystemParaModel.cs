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
        public enum TESTMODE { Normal, Contionus }
  
        //DCTest
        private double _att = 6;
        private double _dcVoltValue = 3.3;
        private double _dcCurrLimit = 0.3;
        private double dc_power1 = -1.59f;
        private double dc_power2 = -1.45f;
        private double dc_power3 = -0.57f;
        private double dc_power4 = -1.10f;
        private string _snDc = "";
        private TESTMODE dc_nTestMode = TESTMODE.Normal;

  
        [CategoryAttribute("FileSetting"), DescriptionAttribute("Set the file path of log")]
        public double Att
        {
            get { return _att; }
            set { _att = value; }
        }
        [CategoryAttribute("DCTest"), DescriptionAttribute("Set DC Value, the uinit is Volt(V)")]
        public double DcVoltValue
        {
            get { return _dcVoltValue; }
            set { _dcVoltValue = value; }
        }
        [CategoryAttribute("DCTest"), DescriptionAttribute("Set current limit, the uinit is Ampere(A)")]
        public double DcCurrLimit
        {
            get { return _dcCurrLimit; }
            set { _dcCurrLimit = value; }
        }
        [CategoryAttribute("DCTest"), DescriptionAttribute("Set sn number")]
        public string SN_DC
        {
            get { return _snDc; }
            set { _snDc = value; }
        }

        [CategoryAttribute("DCTest"), DescriptionAttribute("Set Power1,unit is dbm")]
        public double DcPower1
        {
            get { return dc_power1; }
            set { dc_power1 = value; }
        }
        [CategoryAttribute("DCTest"), DescriptionAttribute("Set Power2,unit is dbm")]
        public double DcPower2
        {
            get { return dc_power2; }
            set { dc_power2 = value; }
        }
        [CategoryAttribute("DCTest"), DescriptionAttribute("Set Power3,unit is dbm")]
        public double DcPower3
        {
            get { return dc_power3; }
            set { dc_power3 = value; }
        }
        [CategoryAttribute("DCTest"), DescriptionAttribute("Set Power4,unit is dbm")]
        public double DcPower4
        {
            get { return dc_power4; }
            set { dc_power4 = value; }
        }
        [CategoryAttribute("DCTest"),
         DescriptionAttribute("Set the test mode,Normal=run once, Continus=run in loops."),
         ReadOnlyAttribute(false),
         BrowsableAttribute(true)
         ]
        public TESTMODE DcTestMode
        {
            get { return dc_nTestMode; }
            set { dc_nTestMode = value; }
        }
       
    }
}
