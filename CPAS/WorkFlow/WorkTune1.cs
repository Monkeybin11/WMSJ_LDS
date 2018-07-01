
using CPAS.Classes;
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
    public class WorkTune1 : WorkFlowBase
    {
        private PrescriptionGridModel Prescription = null;   //配方信息
        private QSerisePlc PLC = null;
        private LDS lds1 = null;
        private LDS lds2 = null;

        //cmd
        private int nCmdHiriz_Grab1=-1;
        private int nCmdHiriz_Grab2 = -1;

        private int nCmdAdjust_Horiz1 = -1;
        private int nCmdAdjust_Horiz2 = -1;

        private bool? bAdjustHoriz1Ok = null;
        private bool? bAdjustHoriz2Ok = null;


        public WorkTune1(WorkFlowConfig cfg) : base(cfg)
        {

        }
        private enum STEP : int
        {
            INIT,

            #region Station 2
            //调水平
            Check_Enable_Adjust_Horiz,  //计算对接角度
            Wait_Horiz_Grab_Cmd,
            Horiz_Grab_Image,
            Cacul_Horiz_Servo_Angle,
            Write_Horiz_Servo_Angle_To_Register,
            Write_Horiz_Grab_Boolean_Result,
            Send_Horiz_Calcu_Angle_Finish_Signal,

                
            Wait_Adjust_Horiz_Cmd,      //计算单次旋转角度      可能需要再分一个线程分开调节
            Turn_On_Laser_And_Light,
            Grab_Laser_Blob,
            Check_Blob_Is_OK,           //判断斑点是否在第四象限
            Cacul_Blob_Angle,
            Write_Blob_Angle_To_Register,
            Write_Blob_Boolean_Result,
            Wait_Servo_Finish_Step,        //——》GrabImage

            Finish_Adjust_Horiz,

           
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
            lds1 = InstrumentMgr.Instance.FindInstrumentByName("LDS[2]") as LDS;
            lds2 = InstrumentMgr.Instance.FindInstrumentByName("LDS[3]") as LDS;
            #endregion

            return true;
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








                    case STEP.DO_NOTHING:
                        PopAndPushStep(STEP.INIT);
                        ShowInfo("就绪");
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

        private async void AdjustHorizProcess(int nIndex)
        {
            if (nIndex != 1 && nIndex != 2)
                throw new Exception($"nIndex now is {nIndex},must be range in [1,2]");
            LDS lds = nIndex == 1 ? lds1 : lds2;
            int nStep = 1;
            await Task.Run(()=> {
                switch (nStep)
                {
                    case 1:
                        break;
                    case 5:
                        break;
                    case 10:
                        break;
                    case 15:
                        break;
                }
            });
        }
    }
}
