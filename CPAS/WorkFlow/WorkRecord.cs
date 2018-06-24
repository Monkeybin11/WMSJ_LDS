using CPAS.Config;
using CPAS.Config.SoftwareManager;
using CPAS.Instrument;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
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
        private enum STEP : int
        {
            INIT,

            OPEN_ALL_RELAY,
            READ_PARA_FROM_UI,
            SET_POWER,
            READ_BACK_CURR,
            READ_ICC,

            CLOSE_RELAY,    //从此初开始循环
            READ_I_DARK_0,
            CALC_I_DARK_1,  //

            SET_ATT,
            OPEN_LIGHT,
            READ_RSSI_0,
            CALC_RSSI_1,
            CALC_RESP,
            OPEN_ALL_RELAY_FINNALY,


            GEN_DATATABLE_FOR_EXCEL,
            SAVE_DATA_TO_FILE,      //结束循环


            EMG,
            EXIT,
            DO_NOTHING,
        }

        protected override bool UserInit()
        {
            Pw1000USB_1 = InstrumentMgr.Instance.FindInstrumentByName("PowerMeter[0]") as PowerMeter;
            Pw1000USB_2 = InstrumentMgr.Instance.FindInstrumentByName("PowerMeter[1]") as PowerMeter;
            return Pw1000USB_1!=null;
        }
        public WorkRecord(WorkFlowConfig cfg) : base(cfg)
        {

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
                        PopAndPushStep(STEP.READ_ICC);
                        ShowPower(EnumUnit.μW);
                        Vision.Vision.Instance.GrabImage(0);
                        Thread.Sleep(100);
                        break;
                    case STEP.READ_ICC:
                        PopAndPushStep(STEP.INIT);
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
