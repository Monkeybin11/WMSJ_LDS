using CPAS.Classes;
using CPAS.Config;
using CPAS.Config.SoftwareManager;
using CPAS.Instrument;
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
        private PowerMeter Pw1000USB_1 = null;
        private PowerMeter Pw1000USB_2 = null;
        private string FILE_FAKE_BARCODE_FILE = FileHelper.GetCurFilePathString() + "UserData\\Barcode.xls";
        private DataTable Fake_Barcode_Dt = new DataTable();
        private enum STEP : int
        {
            INIT=1,

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
            Pw1000USB_1 = InstrumentMgr.Instance.FindInstrumentByName("PowerMeter[0]") as PowerMeter;
            Pw1000USB_2 = InstrumentMgr.Instance.FindInstrumentByName("PowerMeter[1]") as PowerMeter;
            bRet= Pw1000USB_1!=null;
            if (!bRet)
                ShowInfo("初始化失败");
            return bRet;

        }
        public WorkRecord(WorkFlowConfig cfg) : base(cfg)
        {
            #region >>>>读取模块配置信息，初始化工序Enable信息
            LogExcel Fake_Barcode_Excel = new LogExcel(FILE_FAKE_BARCODE_FILE);
            Fake_Barcode_Excel.ExcelToDataTable(ref Fake_Barcode_Dt, "Sheet1");
            foreach (DataRow it in Fake_Barcode_Dt.Rows)
                Console.WriteLine(it["Barcode"]);
            #endregion
            #region >>>>初始化仪表信息

            #endregion
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
                        ShowPower(EnumUnit.μW);
                        Vision.Vision.Instance.GrabImage(0);
                        ShowInfo();
                        Thread.Sleep(100);
                        break;
                    case STEP.DO_NOTHING:
                        PopAndPushStep(STEP.EXIT);
                        ShowPower(EnumUnit.μW);
                        Vision.Vision.Instance.GrabImage(0);
                        Thread.Sleep(100);
                        break;
                  
                    case STEP.EMG:
                        ClearAllStep();
                        break;
                    case STEP.EXIT:
                        return 0;
                }
            }
            return 0;
        }

        private void ShowPower(EnumUnit unit)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Math.Round(Pw1000USB_1.GetPowerValue(EnumUnit.μW), 3).ToString());
            sb.Append(" ");
            sb.Append(unit.ToString());
            sb.Append(",");
            sb.Append(Math.Round(Pw1000USB_1.GetPowerValue(EnumUnit.μW), 3).ToString());
            sb.Append(unit.ToString());
            Messenger.Default.Send<Tuple<string, string, string>>(new Tuple<string, string, string>(cfg.Name, "ShowPower",sb.ToString()), "WorkFlowMessage");
        }
    }
}
