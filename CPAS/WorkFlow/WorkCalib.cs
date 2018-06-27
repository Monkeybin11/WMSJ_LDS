
using CPAS.Config.SoftwareManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPAS.WorkFlow
{
    public class WorkCalib : WorkFlowBase
    {
        private enum STEP : int
        {
            INIT,

            #region Station4

            Check_Enable_Calib,
            Wait_Calib_2m_Cmd,
            Read_Center_Value_2m,
            Write_Calib_2m_Boolean_Result,
            Finish_Calib_2m,
            Wait_Calib_4m_Cmd,
            Read_Center_Value_4m,
            Write_Calib_4m_Boolean_Result,
            Finish_Calib_4m,
            Calclate_From_2m_4m,
            Write_To_LDS,
            Finis_Calib,

            #endregion


            EMG,
            EXIT,
            DO_NOTHING,
        }

        protected override bool UserInit()
        {
            #region >>>>读取模块配置信息，初始化工序Enable信息

            #endregion
            #region >>>>初始化仪表信息

            #endregion
            #region >>>>

            #endregion
            return true;
        }
        public WorkCalib(WorkFlowConfig cfg) : base(cfg)
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
