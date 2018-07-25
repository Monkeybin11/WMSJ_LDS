using CPAS.Classes;
using CPAS.Config;
using CPAS.Config.SoftwareManager;
using CPAS.Instrument;
using CPAS.Models;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPAS.WorkFlow
{
    public class WorkRecord : WorkFlowBase
    {
        private PowerMeter Pw1000USB_1 = null;
        private PowerMeter Pw1000USB_2 = null;
        private LDS lds1 = null;
        private LDS lds2 = null;
        private Keyence_SR1000 BarcodeScanner1 = null;
        private Keyence_SR1000 BarcodeScanner2 = null;
        QSerisePlc PLC = null;

        private string FILE_FAKE_BARCODE_FILE = FileHelper.GetCurFilePathString() + "UserData\\Barcode.xls";
        private DataTable Fake_Barcode_Dt = new DataTable();
        private int nBarcodeInfFileIndex = 0;
        private Task task1 = null, task2 = null;

        protected override bool UserInit()
        {

            bool bRet = false;

            #region >>>>读取模块配置信息，初始化工序Enable信息
            if (GetPresInfomation())
                ShowInfo("加载参数成功");
            else
                ShowInfo("加载参数失败,请确认是否选择参数配方");
            #endregion

            #region >>>>初始化仪表信息
            Pw1000USB_1 = InstrumentMgr.Instance.FindInstrumentByName("PowerMeter[0]") as PowerMeter;
            Pw1000USB_2 = InstrumentMgr.Instance.FindInstrumentByName("PowerMeter[1]") as PowerMeter;
            lds1 = InstrumentMgr.Instance.FindInstrumentByName("LDS[0]") as LDS;
            lds2 = InstrumentMgr.Instance.FindInstrumentByName("LDS[1]") as LDS;
            BarcodeScanner1 = InstrumentMgr.Instance.FindInstrumentByName("SR1000[0]") as Keyence_SR1000;
            BarcodeScanner2 = InstrumentMgr.Instance.FindInstrumentByName("SR1000[1]") as Keyence_SR1000;
            PLC = InstrumentMgr.Instance.FindInstrumentByName("PLC") as QSerisePlc;


            #endregion

            LogExcel Fake_Barcode_Excel = new LogExcel(FILE_FAKE_BARCODE_FILE);
            Fake_Barcode_Excel.ExcelToDataTable(ref Fake_Barcode_Dt, "Sheet1");

            string str = Fake_Barcode_Dt.Rows[0]["Barcode"].ToString();
            str = Fake_Barcode_Dt.Rows[1]["Barcode"].ToString();
            //try
            //{
            //    bool b = Vision.Vision.Instance.ProcessImage(Vision.Vision.IMAGEPROCESS_STEP.GET_ANGLE_TUNE2, 1, null, out object Angle);
            //}
            //catch(Exception ex)
            //{
            //    Messenger.Default.Send<string>(ex.Message, "ShowError");
            //}

            bRet =  true || Pw1000USB_1 != null &&
                    Pw1000USB_2 != null &&
                    lds1 != null &&
                    lds2 != null &&
                    BarcodeScanner1 != null &&
                    BarcodeScanner2 != null &&
                    PLC != null;
            if (!bRet)
                ShowInfo("初始化失败");
            else
            {
                ShowInfo("初始化站位成功");
                if (task1 == null || task1.IsCanceled || task1.IsCompleted)
                {
                    task1 = new Task(() => LdsWorkFunctionSet1(), cts.Token);
                    //task1.Start();
                }
                if (task2 == null || task2.IsCanceled || task2.IsCompleted)
                {
                    task2 = new Task(() => LdsWorkFunctionSet2(), cts.Token);
                    task2.Start();
                }
            }
            return bRet;
        }
        public WorkRecord(WorkFlowConfig cfg) : base(cfg)
        {
            #region >>>>

            #endregion
        }

        protected override int WorkFlow()
        {
            StringBuilder sb = new StringBuilder();
            while (!cts.IsCancellationRequested)
            {
                ShowPower(EnumUnit.μW);
                Thread.Sleep(200);
            }
            return 0;
        }
        private void LdsWorkFunctionSet1()
        {
            int nIndex = 1;
            int nCmd = 0;

            const string cmdReg = "R13";
            const string boolResult_Unlock_Reg = "R15";

            const string barcodeResult_Reg = "R20";
            const string boolResult_Barcode_Reg = "R19";

            const string boolResult_AdjustPower_Reg = "R67";
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    bool bRet = false;
                    nCmd = PLC.ReadInt(cmdReg);
                    switch (nCmd)
                    {
                        case 1:
                            bRet = UnLock(nIndex);
                            PLC.WriteDint(boolResult_Unlock_Reg, bRet ? 2 : 1);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            ShowInfo($"LDS1解锁结果:{bRet}");
                            break;
                        case 3:
                            bRet = GetBarcode(nIndex, out string barcode);
                            PLC.WriteString(barcodeResult_Reg, barcode);
                            PLC.WriteInt(boolResult_Barcode_Reg, bRet ? 2 : 1);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            ShowInfo($"LDS1读取条码结果:{bRet}");
                            break;
                        case 5:
                            bRet = AdjustPowerValue(nIndex);
                            PLC.WriteInt(boolResult_AdjustPower_Reg, bRet ? 2 : 1);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            ShowInfo($"LDS1调整激光功率结果:{bRet}");
                            break;
                        case 100:
                            ReadResutFromPLC(nIndex);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            break;
                        default:
                            break;
                    }
                    Thread.Sleep(30);
                }
                ShowInfo($"LDS1的服务器被用户退出");
            }
            catch (Exception ex)
            {
                ShowInfo($"LDS1的服务器异常终止:{ex.Message}，错误堆栈{ex.StackTrace}");
            }
        }
        private void LdsWorkFunctionSet2()
        {
            int nIndex = 2;
            int nCmd = 0;

            const string cmdReg = "R14";
            const string boolResult_Unlock_Reg = "R16";

            const string barcodeResult_Reg = "R44";
            const string boolResult_Barcode_Reg = "R43";

            const string boolResult_AdjustPower_Reg = "R82";
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    bool bRet = false;
                    nCmd = PLC.ReadInt(cmdReg);
                    switch (nCmd)
                    {
                        case 1:
                            bRet = UnLock(nIndex);
                            PLC.WriteDint(boolResult_Unlock_Reg, bRet ? 2 : 1);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            ShowInfo($"LDS2解锁结果:{bRet}");
                            break;
                        case 3:
                            bRet = GetBarcode(nIndex, out string barcode);
                            PLC.WriteString(barcodeResult_Reg, barcode);
                            PLC.WriteInt(boolResult_Barcode_Reg, bRet ? 2 : 1);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            ShowInfo($"LDS2读取条码结果:{bRet}");
                            break;
                        case 5:
                            bRet = AdjustPowerValue(nIndex);
                            PLC.WriteInt(boolResult_AdjustPower_Reg, bRet ? 2 : 1);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            ShowInfo($"LDS2调整激光功率结果:{bRet}");
                            break;
                        case 100:
                            ReadResutFromPLC(nIndex);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            break;
                        default:
                            break;
                    }
                    Thread.Sleep(30);
                }
                ShowInfo($"LDS1的服务器被用户停止");
            }
            catch (Exception ex)
            {
                ShowInfo($"LDS2的服务器异常终止:{ex.Message}，错误堆栈{ex.StackTrace}");
            }
        }
        private bool UnLock(int nIndex)
        {
            LDS lds = nIndex == 1 ? lds1 : lds2;
            lds.DeInit();
            bool bRet = lds.LdsUnLock(out string error);
            lds.Init();
            return bRet;
        }
        private bool GetBarcode(int nIndex, out string Barcode)
        {
            Barcode= "123456789012345678901";
            return true; 
            LDS lds = nIndex == 1 ? lds1 : lds2;
            Barcode = Prescription.BarcodeSource == PrescriptionGridModel.BARCODESOURCE.SCANNER ?
                               BarcodeScanner1.Getbarcode() : Fake_Barcode_Dt.Rows[nBarcodeInfFileIndex]["Barcode"].ToString();
            return Barcode.Length == Prescription.BarcodeLength;
        }
        private bool AdjustPowerValue(int nIndex)
        {
            return true;
            int nCount = 0;
            bool bRet = false;
            if (nIndex < 1 || nIndex > 2)
                return false;
            LDS lds = nIndex == 1 ? lds1 : lds2;
            PowerMeter powerMeter = nIndex == 1 ? Pw1000USB_1 : Pw1000USB_2;
            double powerValue = powerMeter.GetPowerValue(EnumUnit.μW);
            bool bIncrease = false;
            if (powerValue < Prescription.LDSPower[0])
                bIncrease = true;
            if (powerValue > Prescription.LDSPower[1])
                bIncrease = false;
            while (powerValue < Prescription.LDSPower[0] || powerValue > Prescription.LDSPower[1] || nCount++ > 20)  //直到功率满足要求
            {
                lds.InCreasePower(bIncrease);
                Thread.Sleep(1100); //必须要大于1s
            }
            lds.EnsureLaserPower();
            bRet = lds.CheckSetPowerStatusOK(); //查看是否烧录OK
            return bRet;
        }
        private bool ReadResutFromPLC(int nIndex)
        {
            return true;
        }
        private void ShowPower(EnumUnit unit)   //这个监控是两个一起监控
        {
            return;
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append(Math.Round(Pw1000USB_1.GetPowerValue(EnumUnit.μW), 3).ToString());
            sb.Append(" ");
            sb.Append(unit.ToString());
            sb.Append(",");
            sb.Append(Math.Round(Pw1000USB_1.GetPowerValue(EnumUnit.μW), 3).ToString());
            sb.Append(unit.ToString());
            Messenger.Default.Send(new Tuple<string, string, string>(cfg.Name, "ShowPower", sb.ToString()), "WorkFlowMessage");
        }
    }

}
