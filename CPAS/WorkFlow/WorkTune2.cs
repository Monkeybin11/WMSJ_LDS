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
        private readonly int Cam5=4;
        private readonly int Cam6 = 5;
        //cmd
        private int nCmdFocus_Grab1=-1;
        private int nCmdFocus_Grab2=-1;

        private int nCmdAdjust_Foucs1 = -1;
        private int nCmdAdjust_Foucs2 = -1;
        private bool? bAdjustFocus1Ok = null;
        private bool? bAdjustFocus2Ok = null;

        private CancellationTokenSource ctsMonitorValue1 = null;
        private CancellationTokenSource ctsMonitorValue2 = null;

        private enum STEP : int
        {
            INIT,

            //调焦距
    
            Check_Enable_Adjust_Focus,

            //分支
            Wait_Focus_Grab_Cmd,
            Grab_Focus_Image,
            Cacul_Focus_Servo_Angle,

            Wait_Adjust_Foucs_Cmd,
            Read_Focus_Value,
            Check_Foucs_Is_Ok,

            Turn_A_Circle_Outside_For_Safe,
            Turn_Slowly_inside,
            GetMaxValue,
            Write_MaxValuePos_To_Register,
            TurnBack_MaxValue_Position, //退回最大点处
            Finis_Adjust_Focus,
            //分支


            Wait_Finish_Both,




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
                        ShowInfo("init");
                        Thread.Sleep(200);
                        break;
                    case STEP.Check_Enable_Adjust_Focus:
                        if (Prescription.AdjustFocus)
                        {
                            //PopAndPushStep(STEP.Wait_Focus_Grab_Cmd);
                            AdjustFocusProcess(1);
                            AdjustFocusProcess(2);
                            PopAndPushStep(STEP.Wait_Finish_Both);
                        }
                        else
                        {
                            PopAndPushStep(STEP.DO_NOTHING);
                        }
                        break;

                    case STEP.Wait_Finish_Both:
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

        private async void AdjustFocusProcess(int nIndex)
        {
            if (nIndex != 1 && nIndex != 2)
                throw new Exception($"nIndex now is {nIndex},must be range in [1,2]");
            LDS lds = nIndex == 1 ? lds1 : lds2;
            string strCmdFocusGrabRegister = 1 == nIndex ? "" : "";
            string strCmdFocusStartRegister= 1 == nIndex ? "" : "";


            string strCalAngleJointRegister= 1 == nIndex ? "" : "";
            string strJointBoolResultRegister = 1 == nIndex ? "" : "";
            int nCamID= 1 == nIndex ? Cam5 : Cam6;
            
            int nCmdFocus_Grab = PLC.ReadInt(strCmdFocusGrabRegister);
            int nCmdAdjustStart= PLC.ReadInt(strCmdFocusGrabRegister);

            STEP nStep=STEP.Wait_Focus_Grab_Cmd;
            await Task.Run(() => {
                switch (nStep)
                {
                    case STEP.Wait_Focus_Grab_Cmd:
                        nCmdFocus_Grab = PLC.ReadInt(strCmdFocusGrabRegister);
                        if ((1 == nCmdFocus_Grab))
                                PopAndPushStep(STEP.Grab_Focus_Image);
                        else if (10 == nCmdFocus_Grab)      //如果不需要拍照
                                PopAndPushStep(STEP.INIT);
                        break;
                    case STEP.Grab_Focus_Image:
                        Vision.Vision.Instance.GrabImage(nCamID);
                        PopAndPushStep(STEP.Cacul_Focus_Servo_Angle);
                        break;
                    case STEP.Cacul_Focus_Servo_Angle:
                        if (Vision.Vision.Instance.ProcessImage(Vision.Vision.IMAGEPROCESS_STEP.T1, nCamID, null, out object oResult1))
                        {
                            PLC.WriteDint(strCalAngleJointRegister, Convert.ToInt32(Math.Round(double.Parse(oResult1.ToString()), 3) * 1000)); //角度
                            PLC.WriteInt(strJointBoolResultRegister, 1);    //拍摄1的最终结果
                        }
                        else
                            PLC.WriteInt(strJointBoolResultRegister, 0);    //拍摄1的最终结果
                        PopAndPushStep(STEP.Wait_Adjust_Foucs_Cmd);
                        break;



                    case STEP.Wait_Adjust_Foucs_Cmd:       //有可能拍照全部NG
                        nCmdAdjustStart = PLC.ReadInt(strCmdFocusStartRegister);
                        if (1 == nCmdAdjustStart) 
                             PopAndPushStep(STEP.Read_Focus_Value); 
                        else if (10 == nCmdAdjustStart)
                            PopAndPushStep(STEP.INIT);
                        break;

                    case STEP.Read_Focus_Value:

                        break;
                    case STEP.Check_Foucs_Is_Ok:

                        break;
                    case STEP.Turn_A_Circle_Outside_For_Safe:
                        break;
                    case STEP.Turn_Slowly_inside:
                        break;
                    case STEP.GetMaxValue:
                        break;
                    case STEP.Write_MaxValuePos_To_Register:
                        break;
                    case STEP.TurnBack_MaxValue_Position: //退回最大点处
                        break;
                    case STEP.Finis_Adjust_Focus:
                        break;
                }
            });
        }

        private async void StartMonitor(int nIndex, bool bMonitor = true)
        {
            if(nIndex!=1 && nIndex!=2)
                throw new Exception($"nIndex now is {nIndex},must be range in [1,2]");
            CancellationTokenSource cts = nIndex == 1 ? ctsMonitorValue1 : ctsMonitorValue2;
            if (bMonitor)
            {
                if (cts == null)
                {
                    cts = new CancellationTokenSource();
                    await Task.Run(() =>
                    {
                        StringBuilder sb = new StringBuilder();
                        while (!ctsMonitorPower.IsCancellationRequested)
                        {
                           
                        }
                    }, ctsMonitorPower.Token);
                }
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
