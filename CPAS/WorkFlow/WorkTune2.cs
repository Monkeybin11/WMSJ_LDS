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

        private enum STEP : int
        {
            INIT,

            //调焦距
            #region Station3
            Check_Enable_Adjust_Focus,
            Wait_Focus_Grab_Cmd,
            Grab_Focus_Image,
            Cacul_Focus_Servo_Angle,
            Send_Focus_Calcu_Angle_Finish_Signal,

            Wait_Adjust_Foucs_Cmd,
            Read_Focus_Value,
            Check_Foucs_Is_Ok,
            Adjust_A_Small_Step,
            Read_Focus_Value_For_GetDir,    //先判断方向
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
                        ShowInfo("init");
                        Thread.Sleep(200);
                        break;
                    case STEP.Check_Enable_Adjust_Focus:
                        if (Prescription.AdjustFocus)
                        {
                            PopAndPushStep(STEP.Wait_Focus_Grab_Cmd);
                        }
                        else
                        {
                            PopAndPushStep(STEP.DO_NOTHING);
                        }
                        break;
                    case STEP.Wait_Focus_Grab_Cmd:
                        nCmdFocus_Grab1 = PLC.ReadInt("");
                        nCmdFocus_Grab2 = PLC.ReadInt("");
                        if ((1 == nCmdFocus_Grab1 || 1 == nCmdFocus_Grab2))
                        {
                            nCmdFocus_Grab1 = PLC.ReadInt("");
                            nCmdFocus_Grab2 = PLC.ReadInt("");
                            if (1 == nCmdFocus_Grab1 && 1 == nCmdFocus_Grab2)
                            {
                                PopAndPushStep(STEP.Grab_Focus_Image);
                            }
                        }
                        else if (10 == nCmdFocus_Grab1 && 10 == nCmdFocus_Grab2)
                        {
                            Thread.Sleep(200);
                            nCmdFocus_Grab1 = PLC.ReadInt("");
                            nCmdFocus_Grab2 = PLC.ReadInt("");
                            if (10 == nCmdFocus_Grab1 && 10 == nCmdFocus_Grab2)
                            {
                                PopAndPushStep(STEP.INIT);
                            }
                        }
                        break;
                    case STEP.Grab_Focus_Image:
                        if (1 == nCmdFocus_Grab1)
                        {
                            Vision.Vision.Instance.GrabImage(Cam5);
                        }
                        if (1 == nCmdFocus_Grab2)
                        {
                            Vision.Vision.Instance.GrabImage(Cam6);
                        }
                        PopAndPushStep(STEP.Cacul_Focus_Servo_Angle);
                        break;
                    case STEP.Cacul_Focus_Servo_Angle:
                        if (1 == nCmdFocus_Grab1)
                        {
                            if (Vision.Vision.Instance.ProcessImage(Vision.Vision.IMAGEPROCESS_STEP.T1, Cam5, null, out object oResult1))
                            {
                                PLC.WriteDint("", Convert.ToInt32(Math.Round(double.Parse(oResult1.ToString()), 3) * 1000)); //角度
                                PLC.WriteInt("", 1);    //拍摄1的最终结果
                            }
                            else
                                PLC.WriteInt("", 0);    //拍摄1的最终结果

                        }
                        if (1 == nCmdFocus_Grab2)
                        {
                            if (Vision.Vision.Instance.ProcessImage(Vision.Vision.IMAGEPROCESS_STEP.T1, Cam6, null, out object oResult2))
                            {
                                PLC.WriteDint("", Convert.ToInt32(Math.Round(double.Parse(oResult2.ToString()), 3) * 1000)); //角度
                                PLC.WriteInt("", 1);    //拍摄1的最终结果
                            }
                            else
                                PLC.WriteInt("", 0);    //拍摄2的最终结果
                        }
                        PopAndPushStep(STEP.Send_Focus_Calcu_Angle_Finish_Signal);
                        break;
      
                    case STEP.Send_Focus_Calcu_Angle_Finish_Signal:
                        PLC.WriteInt("", 2);    //完成角度拍摄
                        break;



                    case STEP.Wait_Adjust_Foucs_Cmd:       //有可能拍照全部NG
                        nCmdAdjust_Foucs1 = PLC.ReadInt("");
                        nCmdAdjust_Foucs2= PLC.ReadInt("");
                        if (1 == nCmdAdjust_Foucs1 || 1 == nCmdAdjust_Foucs2)
                        {
                            Thread.Sleep(200);
                            if (1 == nCmdAdjust_Foucs1 || 1 == nCmdAdjust_Foucs2)
                            {
                                PopAndPushStep(STEP.Read_Focus_Value);
                            }
                        }
                        else if (10== nCmdAdjust_Foucs1 && 10== nCmdAdjust_Foucs2)
                        {
                            PopAndPushStep(STEP.INIT);
                        }
                        break;


                    case STEP.Read_Focus_Value:         //开始调整焦距，需要先判断旋转方向
                        break;
                    case STEP.Check_Foucs_Is_Ok:
                        break;
                    case STEP.Adjust_A_Small_Step:
                        break;
                    case STEP.Read_Focus_Value_For_GetDir:
                        break;    //先判断方向
                    case STEP.Write_Angle_To_Register_i:
                        break;
                    case STEP.Wait_Servo_Finish_i:
                        break;
                    case STEP.Read_Focus_Value_i:
                        break;
                    case STEP.Check_Focus_Is_Ok_i:
                        break;

                    case STEP.Finis_Adjust_Focus:
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
            int nStep = 1;
            await Task.Run(() => {
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
