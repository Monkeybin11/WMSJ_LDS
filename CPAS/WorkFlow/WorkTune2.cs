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
    public class WorkTune2 : WorkFlowBase
    {
        private PrescriptionGridModel Prescription = null;   //配方信息
        private QSerisePlc PLC = null;
        private LDS lds1 = null;
        private LDS lds2 = null;
        private enum STEP : int
        {
            INIT,

            //调焦距
            #region Station3
            Check_Enable_Adjust_Focus,      //计算对接角度
            Wait_Focus_Grab_Cmd,
            Grab_Focus_Image,
            Cacul_Focus_Servo_Angle,
            Write_Focus_Servo_To_Register,
            Write_Focus_Grab_Boolean_result,
            Send_Focus_Calcu_Angle_Finish_Signal,

            Wait_Adjust_Foucs_Cmd,
            Read_Focus_Value,
            Check_Foucs_Is_Ok,
            Adjust_A_Small_Step,
            Read_Focus_Value_For_GetDir,    //先判断方向,如果方向固定，那么就可以跳过这一步，大概计算要旋转多少度，根据角度计算每一度大概多少Value
            Write_Angle_To_Register_i,
            Wait_Servo_Finish_i,
            Read_Focus_Value_i,
            Check_Focus_Is_Ok_i,

            Finis_Adjust_Focus,
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
            lds1 = InstrumentMgr.Instance.FindInstrumentByName("LDS[4]") as LDS;
            lds2 = InstrumentMgr.Instance.FindInstrumentByName("LDS[5]") as LDS;
            #endregion

            return true;
        }
        public WorkTune2(WorkFlowConfig cfg) : base(cfg)
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
