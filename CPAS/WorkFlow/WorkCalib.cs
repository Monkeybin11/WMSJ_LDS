
using CPAS.Config;
using CPAS.Config.SoftwareManager;
using CPAS.Instrument;
using CPAS.Models;
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
        private PrescriptionGridModel Prescription = null;   //配方信息
        private QSerisePlc PLC = null;
        private LDS lds1 = null;
        private LDS lds2 = null;

        //cmd
        private int nCmdCalib_2m1 = -1;
        private int nCmdCalib_2m2 = -1;

        private int nCmdCalib_4m1 = -1;
        private int nCmdCalib_4m2 = -1;

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
            Finish_Calib,

            #endregion


            EMG,
            EXIT,
            DO_NOTHING,
        }

        protected override bool UserInit()
        {
            #region >>>>读取模块配置信息，初始化工序Enable信息
            Prescription = ConfigMgr.PrescriptionCfgMgr.Prescriptions[0];
            //sysPara=ConfigMg
            #endregion

            #region >>>>初始化仪表信息
            PLC = InstrumentMgr.Instance.FindInstrumentByName("PLC") as QSerisePlc;
            lds1 = InstrumentMgr.Instance.FindInstrumentByName("LDS[6]") as LDS;
            lds2 = InstrumentMgr.Instance.FindInstrumentByName("LDS[7]") as LDS;
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
                        ShowInfo("Init");
                        Thread.Sleep(200);
                        break;




                    case STEP.Check_Enable_Calib:
                        break;
                    case STEP.Wait_Calib_2m_Cmd:
                        break;
                    case STEP.Read_Center_Value_2m:
                        break;
                    case STEP.Write_Calib_2m_Boolean_Result:
                        break;
                    case STEP.Finish_Calib_2m:
                        break;
                    case STEP.Wait_Calib_4m_Cmd:
                        break;
                    case STEP.Read_Center_Value_4m:
                        break;
                    case STEP.Write_Calib_4m_Boolean_Result:
                        break;
                    case STEP.Finish_Calib_4m:
                        break;
                    case STEP.Calclate_From_2m_4m:
                        break;
                    case STEP.Write_To_LDS:
                        break;
                    case STEP.Finish_Calib:
                        break;






                    case STEP.DO_NOTHING:
                        PopAndPushStep(STEP.INIT);
                        ShowInfo("距离标定工序没有启用");
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
