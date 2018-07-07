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
    public delegate void MonitorValueDelegate(bool bMonitor);
    public class WorkTune2 : WorkFlowBase
    {
        private QSerisePlc PLC = null;
        private LDS lds1 = null;
        private LDS lds2 = null;
        public enum EnumCamID { Cam5=4,Cam6}
        


        private CancellationTokenSource ctsMonitorValue1 = null;
        private CancellationTokenSource ctsMonitorValue2 = null;
        private Task taskMonitorValue1 = null;
        private Task taskMonitorValue2 = null;
        private Dictionary<Int32, int> PosValueDic1 = new Dictionary<Int32, int>();
        private Dictionary<Int32, int> PosValueDic2 = new Dictionary<Int32, int>();


        private int nSubWorkFlowState = 0;
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

            Turn_A_Circle_Outside_For_Safe,
            Wait_CircleOutside_Ok,
            Turn_Slowly_inside,
            Wait_SlowLy_Inside_Ok,
            GetMaxValue,
            Write_MaxValuePos_To_Register,
            Wait_TurnBack_MaxValue_Position, //退回最大点处

            Finish_With_Error,
            Finish_Adjust_Focus,
            //分支


            Wait_Finish_Both,




            EMG,
            EXIT,
            DO_NOTHING,
        }

        protected override bool UserInit()
        {
            #region >>>>读取模块配置信息，初始化工序Enable信息
            if (GetPresInfomation())
                ShowInfo("加载参数成功");
            else
                ShowInfo("加载参数失败,请确认是否选择参数配方");
            #endregion

            #region >>>>初始化仪表信息
            PLC = InstrumentMgr.Instance.FindInstrumentByName("PLC") as QSerisePlc;
            lds1 = InstrumentMgr.Instance.FindInstrumentByName("LDS[4]") as LDS;
            lds2 = InstrumentMgr.Instance.FindInstrumentByName("LDS[5]") as LDS;
            #endregion

            bool bRet= PLC!=null && lds1!=null && lds2!=null && Prescription!=null;
            if(!bRet)
                ShowInfo("初始化失败");

            return bRet;
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
                        PopAndPushStep(STEP.Check_Enable_Adjust_Focus);
                        ShowInfo("init");
                        Thread.Sleep(200);
                        break;
                    case STEP.Check_Enable_Adjust_Focus:
                        if (Prescription.AdjustFocus)
                        {
                            SetSubWorflowState(1, false);
                            SetSubWorflowState(2, false);
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

                        if (GetSubWorkFlowState(1) && GetSubWorkFlowState(2))
                        {
                            //保存结果
                            PopAndPushStep(STEP.INIT);
                        }
                        break;

  

                    case STEP.DO_NOTHING:
                        ShowInfo("该工序未启用");
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
            
           
            string strCmdFocusGrabRegister = 1 == nIndex ? "R211" : "R237";
            string strCalAngleJointAngleRegister= 1 == nIndex ? "R213" : "R239"; //Dint
            string strJointBoolResultRegister = 1 == nIndex ? "R212" : "R238";  //Int

            string strCmdFocusStartRegister = 1 == nIndex ? "R267" : "R288";
            string strAdjustFocusAngleRegister= 1 == nIndex ? "R269" : "R290";   //Dint
            string strAdjustFocusGrabRegister = 1 == nIndex ? "R268" : "R289";   //Int
            string strCmdSingleStepRegister = 1 == nIndex ? "R271" : "R292";   //Int
            
            
            MonitorValueDelegate monitorValueDel = 1 == nIndex? new MonitorValueDelegate(StartMonitor1): new MonitorValueDelegate(StartMonitor2);
            int nCamID= 1 == nIndex ? (int)EnumCamID.Cam5 : (int)EnumCamID.Cam6;
            
            int nCmdFocus_Grab = PLC.ReadInt(strCmdFocusGrabRegister);
            int nCmdAdjustStart= PLC.ReadInt(strCmdFocusGrabRegister);

            Int32 maxPos = 0;
            
            STEP nStep=STEP.Wait_Focus_Grab_Cmd;
            await Task.Run(() => {
                switch (nStep)
                {
                    case STEP.Wait_Focus_Grab_Cmd:
                        nCmdFocus_Grab = PLC.ReadInt(strCmdFocusGrabRegister);
                        if ((1 == nCmdFocus_Grab))
                            PopAndPushStep(STEP.Grab_Focus_Image);
                        else if (10 == nCmdFocus_Grab)      //如果不需要拍照
                        {
                            PopAndPushStep(STEP.Finish_Adjust_Focus);
                        }
                        break;
                    case STEP.Grab_Focus_Image:
                        Vision.Vision.Instance.GrabImage(nCamID);
                        PopAndPushStep(STEP.Cacul_Focus_Servo_Angle);
                        break;
                    case STEP.Cacul_Focus_Servo_Angle:
                        if (Vision.Vision.Instance.ProcessImage(Vision.Vision.IMAGEPROCESS_STEP.T1, nCamID, null, out object oResult1))
                        {
                            PLC.WriteDint(strCalAngleJointAngleRegister, Convert.ToInt32(Math.Round(double.Parse(oResult1.ToString()), 3) * 1000)); //角度
                            PLC.WriteInt(strJointBoolResultRegister, 2);    //拍摄的最终结果
                        }
                        else
                            PLC.WriteInt(strJointBoolResultRegister, 1);    //拍摄的最终结果
                        PopAndPushStep(STEP.Wait_Adjust_Foucs_Cmd);
                        break;



                    case STEP.Wait_Adjust_Foucs_Cmd:       //有可能拍照全部NG
                        nCmdAdjustStart = PLC.ReadInt(strCmdFocusStartRegister);
                        if (1 == nCmdAdjustStart) 
                             PopAndPushStep(STEP.Read_Focus_Value); 
                        else if (10 == nCmdAdjustStart)
                            PopAndPushStep(STEP.Finish_With_Error); 
                        break;

                    case STEP.Read_Focus_Value:
                         if(lds.GetFocusValue()>Prescription.LDSHoriValue6m)    //如果大于直接通过
                            PopAndPushStep(STEP.Finish_Adjust_Focus);
                        else
                            PopAndPushStep(STEP.Turn_A_Circle_Outside_For_Safe);
                        break;
                    case STEP.Turn_A_Circle_Outside_For_Safe:
                        PLC.WriteDint(strAdjustFocusAngleRegister, 360 * 1000);//
                        PLC.WriteInt(strCmdSingleStepRegister, 1);    //表示让伺服开始启动
                        PopAndPushStep(STEP.Wait_CircleOutside_Ok);
                        break;
                    case STEP.Wait_CircleOutside_Ok:
                        if (2 == PLC.ReadInt(strCmdSingleStepRegister))
                            PopAndPushStep(STEP.Turn_Slowly_inside);
                        break;
                    case STEP.Turn_Slowly_inside:       //往里面走多少合适？
                        PLC.WriteDint(strAdjustFocusAngleRegister, -2*360 * 1000);//
                        PLC.WriteInt(strCmdSingleStepRegister, 1);    //表示让伺服开始启动
                        monitorValueDel(true);  //开始监控
                        PopAndPushStep(STEP.Wait_SlowLy_Inside_Ok);
                        break;
                    case STEP.Wait_SlowLy_Inside_Ok:
                        if (2 == PLC.ReadInt(strCmdSingleStepRegister))
                            PopAndPushStep(STEP.GetMaxValue);
                        break;
                    case STEP.GetMaxValue:  //寻找最大值
                        var PosValueDic = 1 == nIndex ? PosValueDic1 : PosValueDic2;
                        var max=PosValueDic.Max(p => p.Value);  //value
                        maxPos = (from dic in PosValueDic where dic.Value == max select dic).ElementAt(0).Key;  //key
                        if(max<Prescription.LDSHoriValue6m)
                            PopAndPushStep(STEP.Finish_With_Error);
                        else
                            PopAndPushStep(STEP.Write_MaxValuePos_To_Register);
                        break;
                    case STEP.Write_MaxValuePos_To_Register:
                        //用最大位置减去当前位置
                        PLC.WriteDint(strAdjustFocusAngleRegister, maxPos - 10000);    //ddddd
                        PLC.WriteInt(strCmdSingleStepRegister, 1);    //表示让伺服开始启动
                        PopAndPushStep(STEP.Wait_TurnBack_MaxValue_Position);
                        break;
                    case STEP.Wait_TurnBack_MaxValue_Position: //退回最大点处
                        if (2 == PLC.ReadInt(strCmdSingleStepRegister))
                            PopAndPushStep(STEP.Finish_Adjust_Focus);
                        break;

                    case STEP.Finish_With_Error:
                        //错误处理
                        PopAndPushStep(STEP.Finish_Adjust_Focus);  //完成
                        break;
                    case STEP.Finish_Adjust_Focus:
                        //证确结果保存
                        PLC.WriteInt(strCmdFocusStartRegister, 2);  //完成
                        SetSubWorflowState(nIndex, true);
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
                    taskMonitorValue1 = new Task(()=> {
                        while (!ctsMonitorValue1.Token.IsCancellationRequested)
                        {
                            Thread.Sleep(50);
                            int value=lds1.GetFocusValue();
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
                    taskMonitorValue2 = new Task(() => {
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
        private void SetSubWorflowState(int nIndex, bool bFinish)
        {
            int nState1 = nIndex == 1 ? 1 : 0;
            int nState2 = nIndex == 1 ? 1 : 0;
            nSubWorkFlowState = nState1 + (nState2 << 1);
        }
        private bool GetSubWorkFlowState(int nIndex)
        {
            return 1 == ((nSubWorkFlowState >> (nIndex - 1)) & 0x01);
        }
    }
}
