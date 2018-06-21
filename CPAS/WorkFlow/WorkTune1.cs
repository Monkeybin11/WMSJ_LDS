
using CPAS.Config.SoftwareManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPAS.WorkFlow
{
    public class WorkTune1 : WorkFlowBase
    {
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
            return true;
        }
        public  WorkTune1(WorkFlowConfig cfg) : base(cfg)
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
                        ShowInfo("12422435");
                        Thread.Sleep(200);
                        break;
                    case STEP.DO_NOTHING:
                        PopAndPushStep(STEP.INIT);
                        ShowInfo("jksjfkjfiwf");
                        Thread.Sleep(200);
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
    }
}
