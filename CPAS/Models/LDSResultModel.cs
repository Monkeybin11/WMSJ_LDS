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
    public class LDSResultModel
    {
        private StringBuilder sb = null;
        public LDSResultModel()
        {
            sb = new StringBuilder();
            SN = null;
            UnLockOk = null;
            ReadBarcodeOK = null;
            AdjustLaserOK = null;
            AdjustHorizOK = null;
            AdjustFocusOK = null;
            CalibrationOK = null;
        }
        public string SN { set; get; }
        public bool? UnLockOk { get; set; }
        public bool? ReadBarcodeOK{get;set;}
        public bool? AdjustLaserOK   { get; set; }
        public bool? AdjustHorizOK   { get; set; }
        public bool? AdjustFocusOK   { get; set; }
        public bool? CalibrationOK   { get; set; }

        public void SaveResult(EnumTestStation stationTest,bool bResult)
        {
            switch (stationTest)
            {
                case EnumTestStation.UnLock:
                    UnLockOk = bResult;
                    break;
                case EnumTestStation.ReadBarcode:
                    ReadBarcodeOK = bResult;
                    break;
                case EnumTestStation.AdjustLaser:
                    AdjustLaserOK = bResult;
                    break;
                case EnumTestStation.AdjustHoriz:
                    AdjustHorizOK = bResult;
                    break;
                case EnumTestStation.AdjustFocus:
                    AdjustFocusOK = bResult;
                    break;
                case EnumTestStation.Calibration:
                    CalibrationOK = bResult;
                    break;
                default:
                    break;
            }
        }
        public string GetLastResult()
        {
            //条码标识
            if (SN == null)
                sb.Append("SN:未测试&");
            else
                sb.Append($"SN:{SN}&");

            //解锁结果
            if (UnLockOk == null)
                sb.Append("解锁:未测试&");
            else
                sb.Append($"解锁:{UnLockOk}&");

            //读条码结果
            if (ReadBarcodeOK == null)
                sb.Append("扫码:未测试&");
            else
                sb.Append($"扫码:{ReadBarcodeOK}&");

            //调激光结果
            if (AdjustLaserOK == null)
                sb.Append("调激光:未测试&");
            else
                sb.Append($"调激光:{AdjustLaserOK}&");

            //调水平结果
            if (AdjustHorizOK == null)
                sb.Append("调水平:未测试&");
            else
                sb.Append($"调水平:{AdjustHorizOK}&");
            
            //调焦距结果
            if (AdjustFocusOK == null)
                sb.Append("调焦距:未测试&");
            else
                sb.Append($"调焦距:{AdjustFocusOK}&");

            //标定结果
            if (CalibrationOK == null)
                sb.Append("标定:未测试&");
            else
                sb.Append($"标定:{CalibrationOK}");



            return sb.ToString() ;
        }

    }
}
