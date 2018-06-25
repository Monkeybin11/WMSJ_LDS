using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Models
{
    public enum EnumTestStation
    {
        UnLock,
        ReadBarcode,
        AdjustLaser,
        AdjustHoriz,
        AdjustFocus,
        Calibration,
    }
    public class LDSModel
    {
        private StringBuilder sb = null;
        public LDSModel()
        {
            sb = new StringBuilder();
        }
        public string SN { set; get; }
        public bool? UnLockOk { get; set; }
        public bool? ReadBarcodeOK{get;set;}
        public bool? AdjustLaserOK   { get; set; }
        public bool? AdjustHorizOK   { get; set; }
        public bool? AdjustFocusOK   { get; set; }
        public bool? CalibrationOK   { get; set; }
        public void SaveResult(EnumTestStation stationTest,bool bOk)
        {
            switch (stationTest)
            {
                case EnumTestStation.UnLock:
                    UnLockOk = bOk;
                    break;
                case EnumTestStation.ReadBarcode:
                    ReadBarcodeOK = bOk;
                    break;
                case EnumTestStation.AdjustLaser:
                    AdjustLaserOK = bOk;
                    break;
                case EnumTestStation.AdjustHoriz:
                    AdjustHorizOK = bOk;
                    break;
                case EnumTestStation.AdjustFocus:
                    AdjustFocusOK = bOk;
                    break;
                case EnumTestStation.Calibration:
                    CalibrationOK = bOk;
                    break;
                default:
                    break;
            }
            sb.Append(stationTest.ToString());
            sb.Append(bOk ? "OK" : "NG");
            sb.Append(",");
        }
        public string GetLastResult()
        {
            return sb.ToString() ;
        }

    }
}
