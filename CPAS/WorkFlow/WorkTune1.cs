
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
        public enum EnumCamID { Cam1, Cam2, Cam3, Cam4 };
        //cmd
        private int nCmdHiriz_Grab1 = -1;
        private int nCmdHiriz_Grab2 = -1;

        private int nCmdAdjust_Horiz1 = -1;
        private int nCmdAdjust_Horiz2 = -1;

        private bool? bAdjustHoriz1Ok = null;
        private bool? bAdjustHoriz2Ok = null;


        //Monitor
        private CancellationTokenSource ctsMonitorValue1 = null;
        private CancellationTokenSource ctsMonitorValue2 = null;
        private Task taskMonitorValue1 = null;
        private Task taskMonitorValue2 = null;
        private Dictionary<Int32, int> PosValueDic1 = new Dictionary<Int32, int>();
        private Dictionary<Int32, int> PosValueDic2 = new Dictionary<Int32, int>();

        public WorkTune1(WorkFlowConfig cfg) : base(cfg)
        {

        }
        private enum STEP : int
        {
            INIT,


            //调水平
            Check_Enable_Adjust_Horiz,  //计算对接角度

            //从此开始分支

            Wait_Horiz_Grab_Cmd,
            Horiz_Grab_Image,
            Cacul_Horiz_Servo_Angle,
            Send_Horiz_Calcu_Angle_Finish_Signal,


            Wait_Adjust_Horiz_Cmd,      //计算单次旋转角度      可能需要再分一个线程分开调节

            Grab_Laser_Blob,
            Cacul_Blob_Angle,
            Wait_Servo_Finish_Step,        //——》GrabImage   //先旋转到X轴附近，再旋转90度，找Max，然后通知伺服回头

            Turn90Degree,
            Wait90DegreeOk,
            FindMaxValue,

            TurnBackToMaxPos,
            WaitTurnMaxPos_Ok,
            
            Select_6m_Target,
            Wait_Prepare_6m_Ok,
            Check_6m_Value_IsOk,
            Adjust_6m,          //如何调整6米出的强度值   ddddd 待完成


            Finish_Adjust_Horiz,


            Finish_With_Error,
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
            lds1 = InstrumentMgr.Instance.FindInstrumentByName("LDS[2]") as LDS;
            lds2 = InstrumentMgr.Instance.FindInstrumentByName("LDS[3]") as LDS;
            #endregion

            return PLC != null && lds1 != null && lds2 != null;
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

                    case STEP.Check_Enable_Adjust_Horiz:
                        if (Prescription.AdjustFocus)
                        {
                            AdjustHorizProcess(1);
                            AdjustHorizProcess(2);
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

        private async void AdjustHorizProcess(int nIndex)
        {
            if (nIndex != 1 && nIndex != 2)
                throw new Exception($"nIndex now is {nIndex},must be range in [1,2]");
            LDS lds = nIndex == 1 ? lds1 : lds2;
            int CamTopID = nIndex == 1 ? (int)EnumCamID.Cam1 : (int)EnumCamID.Cam2;
            int CamBottonCam = nIndex == 1 ? (int)EnumCamID.Cam3 : (int)EnumCamID.Cam4;
            //对接角度
            string cmdGrab_Start_Reg = nIndex == 1 ? "R107" : "R134";   //int
            string boolResult_Grab_Reg = nIndex == 1 ? "R108" : "R137"; //int
            string joint_Angle_Reg = nIndex == 1 ? "R109" : "R135";  //Dint

            //调水平
            string cmdAdjust_Start_Reg = nIndex == 1 ? "R162" : "R180";   //int  PLC--->PC   
            string selectTarget_Reg = nIndex == 1 ? "R161" : "R179";   //int 告诉PLC当前要进行哪一个调整
            string adjustAngle_Reg = nIndex == 1 ? "R164" : "R182";   //Dint
            string adjustBool_Result_Reg = nIndex == 1 ? "R163" : "R181";   //int
            string cmd_Single_Step_Reg = nIndex == 1 ? "R166" : "R184";   //int
            MonitorValueDelegate monitorValueDel = 1 == nIndex ? new MonitorValueDelegate(StartMonitor1) : new MonitorValueDelegate(StartMonitor2);
            Int32 maxPos2m = 0,maxPos6m=0;

            STEP nStep = STEP.Wait_Horiz_Grab_Cmd;
            await Task.Run(() =>
            {
                switch (nStep)
                {
                    case STEP.Wait_Horiz_Grab_Cmd:
                        if (1 == PLC.ReadInt(cmdGrab_Start_Reg))
                        {
                            PopAndPushStep(STEP.Horiz_Grab_Image);
                        }
                        else if (10 == PLC.ReadInt(cmdGrab_Start_Reg))
                        {
                            PLC.WriteInt(cmdGrab_Start_Reg, 2);
                            PopAndPushStep(STEP.Finish_Adjust_Horiz);
                        }
                        break;
                    case STEP.Horiz_Grab_Image:
                        Vision.Vision.Instance.GrabImage(CamTopID);
                        break;
                    case STEP.Cacul_Horiz_Servo_Angle:
                        if (Vision.Vision.Instance.ProcessImage(Vision.Vision.IMAGEPROCESS_STEP.T1, CamTopID, null, out object oResult1))
                        {
                            PLC.WriteDint(joint_Angle_Reg, Convert.ToInt32(Math.Round(double.Parse(oResult1.ToString()), 3) * 1000));
                            PLC.WriteInt(boolResult_Grab_Reg, 2);
                            PopAndPushStep(STEP.Finish_With_Error);
                        }
                        else
                        {
                            PLC.WriteInt(boolResult_Grab_Reg, 1);
                            PopAndPushStep(STEP.Finish_With_Error);
                        }
                        break;

                    case STEP.Send_Horiz_Calcu_Angle_Finish_Signal:
                        PLC.WriteInt(cmdGrab_Start_Reg, 2);
                        PopAndPushStep(STEP.Wait_Adjust_Horiz_Cmd);
                        break;


                    case STEP.Wait_Adjust_Horiz_Cmd:            //等待调水平开始，PLC此步需要打开光源控制器
                        if (1 == PLC.ReadInt(cmdAdjust_Start_Reg))
                        {
                            PLC.WriteInt(selectTarget_Reg, 1);   //选择2米的标靶
                            PLC.WriteInt(cmdAdjust_Start_Reg, 2);   //调水平启动中
                            PopAndPushStep(STEP.Finish_Adjust_Horiz);
                        }
                        else if (10 == PLC.ReadInt(cmdAdjust_Start_Reg))
                        {
                            PopAndPushStep(STEP.Grab_Laser_Blob);
                        }
                        break;

                    case STEP.Grab_Laser_Blob:      //拍照，先移动到X轴或者Y轴上面，再旋转90即可，此过程需要实时监控 Pos 与 Value
                        Vision.Vision.Instance.GrabImage(CamBottonCam);
                        break;
                    case STEP.Cacul_Blob_Angle:
                        if (Vision.Vision.Instance.ProcessImage(Vision.Vision.IMAGEPROCESS_STEP.T1, CamBottonCam, null, out object oResult))
                        {
                            PLC.WriteDint(adjustAngle_Reg, Convert.ToInt32(Math.Round(double.Parse(oResult.ToString()), 3) * 1000));
                            PLC.WriteInt(cmd_Single_Step_Reg, 1);
                            PopAndPushStep(STEP.Wait_Servo_Finish_Step);
                        }
                        else
                        {
                            PLC.WriteInt(adjustBool_Result_Reg, 1);
                            PopAndPushStep(STEP.Finish_With_Error);
                        }
                        break;

                    case STEP.Wait_Servo_Finish_Step:
                        if (2 == PLC.ReadInt(cmd_Single_Step_Reg))  //等待伺服到达X/Y轴
                        {
                            monitorValueDel(true);  //开始监测数据
                            PLC.WriteInt(cmd_Single_Step_Reg, 0);
                            PopAndPushStep(STEP.Turn90Degree);
                        }

                        break;                              //——》GrabImage   //先旋转到X轴附近，再旋转90度，找Max，然后通知伺服回头
   
                    case STEP.Turn90Degree:
                        PLC.WriteDint(adjustAngle_Reg, Convert.ToInt32(90 * 1000));
                        PLC.WriteInt(cmd_Single_Step_Reg, 1);
                        PopAndPushStep(STEP.Turn90Degree);
                        break;
                    case STEP.Wait90DegreeOk:
                        if (2 == PLC.ReadInt(cmd_Single_Step_Reg))
                        {
                            monitorValueDel(false); //关闭检测
                            PopAndPushStep(STEP.FindMaxValue);
                        }
                        break;
                    case STEP.FindMaxValue:
                        var PosValueDic = 1 == nIndex ? PosValueDic1 : PosValueDic2;
                        UInt16 max = (UInt16)PosValueDic.Max(p => p.Value);  //value
                        maxPos2m = (from dic in PosValueDic where dic.Value == max select dic).First().Key;  //key
                        if (max > Prescription.LDSHoriValue2m[0] && max< Prescription.LDSHoriValue2m[1])    //满足条件旋转到最大值处，等待调整6.5米
                            PopAndPushStep(STEP.TurnBackToMaxPos);
                        else
                            PopAndPushStep(STEP.Finish_With_Error);
                        break;

                    case STEP.TurnBackToMaxPos:
                        PLC.WriteDint(adjustAngle_Reg, maxPos2m);
                        PLC.WriteInt(cmd_Single_Step_Reg, 1);
                        PopAndPushStep(STEP.WaitTurnMaxPos_Ok);
                        break;
                    case STEP.WaitTurnMaxPos_Ok:
                        if (2 ==PLC.ReadInt(cmd_Single_Step_Reg))
                        {
                            PLC.WriteInt(cmd_Single_Step_Reg, 0);
                            PopAndPushStep(STEP.Select_6m_Target);
                        }
                        break;

                    case STEP.Select_6m_Target:
                        PLC.WriteInt(selectTarget_Reg, 2);   //选择6米的标靶  //ddddd
                        PopAndPushStep(STEP.Wait_Prepare_6m_Ok);
                        break;
                    case STEP.Wait_Prepare_6m_Ok:       //判断条件是什么
                        PopAndPushStep(STEP.Check_6m_Value_IsOk);
                        break;
                    case STEP.Check_6m_Value_IsOk:
                        if (lds.GetExposeValue() > Prescription.LDSHoriValue6m) //符合要求
                        {
                            PLC.WriteInt(adjustBool_Result_Reg, 2);
                            PopAndPushStep(STEP.Finish_Adjust_Horiz);
                        }
                        else
                        {
                            PLC.WriteInt(adjustBool_Result_Reg, 1); //两米的结果和6米的结果需要分开？？？？？
                            PopAndPushStep(STEP.Finish_Adjust_Horiz);
                        }
                        break;
                    




                    case STEP.Finish_With_Error:
                        //错误处理

                        break;
                    case STEP.Finish_Adjust_Horiz:

                        //正常结束处理数据
                        break;
                }
            });
        }
        private void StartMonitor1(bool bMonitor = true)
        {
            if (bMonitor)
            {
                if (taskMonitorValue1 == null || taskMonitorValue1.IsCanceled || taskMonitorValue1.IsCompleted)
                {
                    PosValueDic1.Clear();
                    ctsMonitorValue1 = new CancellationTokenSource();
                    taskMonitorValue1 = new Task(() =>
                    {
                        while (!ctsMonitorValue1.Token.IsCancellationRequested)
                        {
                            Thread.Sleep(50);
                            int value = lds1.GetFocusValue();
                            Int32 pos = PLC.ReadDint(""); //读取实时位置
                            PosValueDic1.Add(pos, value);
                        }

                    }, ctsMonitorValue1.Token);
                }
            }
            else
            {
                if (ctsMonitorValue1 != null)
                {
                    ctsMonitorValue1.Cancel();
                }
            }
        }
        private void StartMonitor2(bool bMonitor = true)
        {
            if (bMonitor)
            {
                if (taskMonitorValue2 == null || taskMonitorValue2.IsCanceled || taskMonitorValue2.IsCompleted)
                {
                    PosValueDic2.Clear();
                    ctsMonitorValue2 = new CancellationTokenSource();
                    taskMonitorValue2 = new Task(() =>
                    {
                        while (!ctsMonitorValue2.Token.IsCancellationRequested)
                        {
                            Thread.Sleep(50);
                            int value = lds2.GetFocusValue();
                            Int32 pos = PLC.ReadDint(""); //读取实时位置
                            PosValueDic2.Add(pos, value);
                        }

                    }, ctsMonitorValue2.Token);
                }
            }
            else
            {
                if (ctsMonitorValue2 != null)
                {
                    ctsMonitorValue2.Cancel();
                }
            }
        }
    }
}
