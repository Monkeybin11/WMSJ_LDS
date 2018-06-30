using CPAS.Classes;
using CPAS.Config;
using CPAS.Config.SoftwareManager;
using CPAS.Instrument;
using CPAS.Models;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPAS.WorkFlow
{
    public class WorkRecord : WorkFlowBase
    {
        PrescriptionGridModel Prescription = null;   //配方信息
        private PowerMeter Pw1000USB_1 = null;
        private PowerMeter Pw1000USB_2 = null;
        private LDS lds1 = null;
        private LDS lds2 = null;
        private Keyence_SR1000 BarcodeScanner1 = null;
        private Keyence_SR1000 BarcodeScanner2 = null;
        SystemParaModel sysPara = null;
        QSerisePlc PLC = null;
        CancellationTokenSource ctsMonitorPower = null;
        private string FILE_FAKE_BARCODE_FILE = FileHelper.GetCurFilePathString() + "UserData\\Barcode.xls";
        private DataTable Fake_Barcode_Dt = new DataTable();

        //UnLock cmd
        private int nCmdUnLock1 = -1;
        private int nCmdUnLock2 = -1;

        //scan barcode cmd
        private int nCmdScanbarcode1=-1;
        private int nCmdScanbarcode2 = -1;
        private string strBarcode1 = "";
        private string strBarcode2 = "";

        //Adjust laser cmd
        private int nCmdAdjustLaser1 = -1;
        private int nCmdAdjustLaser2 = -1;

        private enum STEP : int
        {
            INIT = 1,

            #region Station 1
            Check_Enable_UnLock,
            Wait_UnLock_Cmd,
            Write_Unlock_Result,

            Check_Enable_ScanBarcode,
            Wait_Scan_Barcode_Cmd,
            Write_Barcode_To_Register,
            Write_Scan_Result,

            Check_Enable_Adjust_Laser_Power,
            Wait_Adjust_Laser_Power_Cmd,
            Write_Adjust_Laser_Power_Result,
            #endregion

            EMG,
            EXIT,
            DO_NOTHING,
        }

        protected override bool UserInit()
        {
            bool bRet = false;

            #region >>>>读取模块配置信息，初始化工序Enable信息
            Prescription = ConfigMgr.PrescriptionCfgMgr.Prescriptions[0];
            //sysPara=ConfigMg
            #endregion

            #region >>>>初始化仪表信息
            Pw1000USB_1 = InstrumentMgr.Instance.FindInstrumentByName("PowerMeter[0]") as PowerMeter;
            Pw1000USB_2 = InstrumentMgr.Instance.FindInstrumentByName("PowerMeter[1]") as PowerMeter;
            lds1 = InstrumentMgr.Instance.FindInstrumentByName("LDS[0]") as LDS;
            lds2 = InstrumentMgr.Instance.FindInstrumentByName("LDS[1]") as LDS;
            BarcodeScanner1 = InstrumentMgr.Instance.FindInstrumentByName("SR1000[0]") as Keyence_SR1000;
            BarcodeScanner2 = InstrumentMgr.Instance.FindInstrumentByName("SR1000[1]") as Keyence_SR1000;
            PLC = InstrumentMgr.Instance.FindInstrumentByName("PLC") as QSerisePlc;

#if TEST
            string strTest = "ABCDEFGHIJKPRicky124567IUTVNghj";
            PLC.WriteString("R100", strTest);
            string str = PLC.ReadString("R100", strTest.Length);
#endif

#endregion

            LogExcel Fake_Barcode_Excel = new LogExcel(FILE_FAKE_BARCODE_FILE);
            Fake_Barcode_Excel.ExcelToDataTable(ref Fake_Barcode_Dt, "Sheet1");

            bRet =  Pw1000USB_1 != null &&
                    Pw1000USB_2 != null &&
                    lds1 != null &&
                    lds2 != null &&
                    BarcodeScanner1 != null &&
                    BarcodeScanner2 != null &&
                    PLC !=null;
            if (!bRet)
                ShowInfo("初始化失败");

            return bRet;

        }
        public WorkRecord(WorkFlowConfig cfg) : base(cfg)
        {
            
        
            #region >>>>

            #endregion
        }

        protected override int WorkFlow()
        {
            ClearAllStep();
            PushStep(STEP.INIT);
            while (!cts.IsCancellationRequested)
            {
                nStep = PeekStep();
                switch (nStep)
                {
                    case STEP.INIT:
                        PopAndPushStep(STEP.DO_NOTHING);
                        Vision.Vision.Instance.GrabImage(0);
                        ShowInfo();
                        Thread.Sleep(100);
                        break;

                    #region >>>>解锁
                    case STEP.Check_Enable_UnLock:
                        if (Prescription.UnLock)
                        {

                            PopAndPushStep(STEP.Wait_UnLock_Cmd);
                        }
                        else
                        {
                            PopAndPushStep(STEP.Check_Enable_ScanBarcode);//否则直接跳到等待扫码工序
                        }
                        break;
                    case STEP.Wait_UnLock_Cmd:
                        nCmdUnLock1 = PLC.ReadInt("");
                        nCmdUnLock2 = PLC.ReadInt("");
                        if (1 == nCmdUnLock1 || 1 == nCmdUnLock2)
                        {
                            Thread.Sleep(200);
                            nCmdUnLock1 = PLC.ReadInt("");
                            nCmdUnLock2 = PLC.ReadInt("");
                            if (1 == nCmdUnLock1 || 1 == nCmdUnLock2)
                            {
                                if (1 == nCmdUnLock1)
                                {
                                    //解锁1
                                }
                                if (1 == nCmdUnLock2)
                                {
                                    //解锁2
                                }
                                PopAndPushStep(STEP.Write_Unlock_Result);
                            }
                        }
                        break;
                    case STEP.Write_Unlock_Result:
                        if (1 == nCmdUnLock1)
                            PLC.WriteInt("", 1);
                        if (1 == nCmdUnLock2)
                            PLC.WriteInt("", 1);
                        PopAndPushStep(STEP.Check_Enable_ScanBarcode);
                        break;
                    #endregion


                    #region >>>>扫码
                    case STEP.Check_Enable_ScanBarcode:
                        if (Prescription.ReadBarcode)
                        {
                            PopAndPushStep(STEP.Wait_Scan_Barcode_Cmd);
                        }
                        else
                        {
                            PopAndPushStep(STEP.Check_Enable_Adjust_Laser_Power);//否则直接跳到调激光工序
                        }
                        break;
                    case STEP.Wait_Scan_Barcode_Cmd:
                        nCmdScanbarcode1 = PLC.ReadInt("");
                        nCmdScanbarcode2 = PLC.ReadInt("");
                        if (1 == nCmdScanbarcode1 || 1 == nCmdScanbarcode2)
                        {
                            PLC.WriteInt("", 2);
                            PLC.WriteInt("", 2);
                            PopAndPushStep(STEP.Write_Barcode_To_Register);
                        }
                        break;
                    case STEP.Write_Barcode_To_Register:
                        if (nCmdScanbarcode1 == 1)
                        {
                            strBarcode1 = BarcodeScanner1.Getbarcode();
                            PLC.WriteString("", strBarcode1);
                        }
                        if (nCmdScanbarcode2 == 1)
                        {
                            strBarcode2 = BarcodeScanner1.Getbarcode();
                            PLC.WriteString("", strBarcode2);
                        }
                        PopAndPushStep(STEP.Write_Scan_Result);
                        break;
                    case STEP.Write_Scan_Result:
                        if (nCmdScanbarcode1 == 1)
                        {
                            if(Prescription.BarcodeLength== strBarcode1.Length)
                                PLC.WriteInt("", 1);    //条码1结果码1结果
                            else
                                PLC.WriteInt("", 0);    //条码1结果码1结果
                        }
                        if (nCmdScanbarcode2 == 1)
                        {
                            if (Prescription.BarcodeLength == strBarcode2.Length)
                                PLC.WriteInt("", 1);    //条码2结果
                            else
                                PLC.WriteInt("", 0);    //条码2结果
                        }
                        PopAndPushStep(STEP.Check_Enable_Adjust_Laser_Power);
                        break;
                    #endregion

                    #region >>>>调激光功率
                    case STEP.Check_Enable_Adjust_Laser_Power:
                        if (Prescription.AdjustLaser)
                        {
                            ShowPower(EnumUnit.μW, true);   //实时显示功率
                        }
                        else
                        {
                            PopAndPushStep(STEP.INIT);//测试完毕，直接跳到开始
                        }
                        break;
                    case STEP.Wait_Adjust_Laser_Power_Cmd:
                        nCmdAdjustLaser1 = PLC.ReadInt("");
                        nCmdAdjustLaser2 = PLC.ReadInt("");
                        if (nCmdAdjustLaser1 == 1 || nCmdAdjustLaser1 == 1)
                        {
                            Thread.Sleep(200);
                            nCmdAdjustLaser1 = PLC.ReadInt("");
                            nCmdAdjustLaser2 = PLC.ReadInt("");
                            if (nCmdAdjustLaser1 == 1 || nCmdAdjustLaser1 == 1)
                            {

                                PopAndPushStep(STEP.Write_Adjust_Laser_Power_Result);
                            }
                        }
                        break;

                    //中间缺少调功率过程
                    case STEP.Write_Adjust_Laser_Power_Result:
                        ShowPower(EnumUnit.μW, false);  //关闭激光调整
                        break;
                    #endregion

                    case STEP.DO_NOTHING:   //调试使用
                        Thread.Sleep(100);
                        break;

                    case STEP.EMG:
                        ClearAllStep();
                        break;
                    case STEP.EXIT:
                        ShowPower(EnumUnit.μW, false);
                        return 0;
                }
            }
            ShowPower(EnumUnit.μW, false);
            return 0;
        }

        private async void ShowPower(EnumUnit unit, bool bMonitor = true)
        {
            if (bMonitor)
            {
                ctsMonitorPower = new CancellationTokenSource();
                await Task.Run(() =>
                {
                    StringBuilder sb = new StringBuilder();
                    while (!ctsMonitorPower.IsCancellationRequested)
                    {
                        sb.Clear();
                        sb.Append(Math.Round(Pw1000USB_1.GetPowerValue(EnumUnit.μW), 3).ToString());
                        sb.Append(" ");
                        sb.Append(unit.ToString());
                        sb.Append(",");
                        sb.Append(Math.Round(Pw1000USB_1.GetPowerValue(EnumUnit.μW), 3).ToString());
                        sb.Append(unit.ToString());
                        Messenger.Default.Send<Tuple<string, string, string>>(new Tuple<string, string, string>(cfg.Name, "ShowPower", sb.ToString()), "WorkFlowMessage");
                    }
                }, ctsMonitorPower.Token);
            }
            else
            {
                if (ctsMonitorPower != null)
                {
                    ctsMonitorPower.Cancel();
                    ctsMonitorPower = null;
                }
            }
        }
    }
}
