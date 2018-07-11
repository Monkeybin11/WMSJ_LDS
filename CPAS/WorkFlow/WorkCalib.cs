
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
        
        private QSerisePlc PLC = null;
        private LDS lds1 = null;
        private LDS lds2 = null;


        private int nSubWorkFlowState=0;
        private enum STEP : int
        {
            INIT,



            Check_Enable_Calib,     //分支
            

            Wait_Calib_2m_Cmd,
            Read_Center_Value_2m,
            Write_Calib_2m_Boolean_Result,
            Finish_Calib_2m,
            Wait_Calib_4m_Cmd,
            Read_Center_Value_4m,
            Write_Calib_4m_Boolean_Result,
            Finish_Calib_4m,
            Calclate_From_2m_4m,

            Finish_With_Error,
            Finish_Calib,

          
            Wait_Finish_Both,

            EMG,
            EXIT,
            DO_NOTHING,
        }

        protected override bool UserInit()
        {
            #region >>>>读取模块配置信息，初始化工序Enable信息
            if(GetPresInfomation())
                ShowInfo("加载参数成功");
            else
                ShowInfo("加载参数失败,请确认是否选择参数配方");
            #endregion

            #region >>>>初始化仪表信息
            PLC = InstrumentMgr.Instance.FindInstrumentByName("PLC") as QSerisePlc;
            lds1 = InstrumentMgr.Instance.FindInstrumentByName("LDS[6]") as LDS;
            lds2 = InstrumentMgr.Instance.FindInstrumentByName("LDS[7]") as LDS;
            #endregion

            bool bRet = PLC != null && lds1 != null && lds2 != null && Prescription != null;
            if (!bRet)
                ShowInfo("初始化失败");
            return bRet;
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
                        if (Prescription.EnableCalibration)
                        {
                            SetSubWorflowState(1, false);
                            SetSubWorflowState(2, false);
                            CalibProcess(1);
                            CalibProcess(2);
                            PopAndPushStep(STEP.Wait_Calib_2m_Cmd);
                        }
                        else
                            PopAndPushStep(STEP.DO_NOTHING);
                        break;



                    case STEP.Wait_Finish_Both:
                        if (GetSubWorkFlowState(1) && GetSubWorkFlowState(2))
                        {
                            PopAndPushStep(STEP.INIT);
                        }
                        break;



                    case STEP.DO_NOTHING:
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
        private async void CalibProcess(int nIndex)
        {
            if (nIndex != 1 && nIndex != 2)
                throw new Exception($"nIndex now is {nIndex},must be range in [1,2]");


            LDS lds = nIndex == 1 ? lds1 : lds2;
            string cmdCalib2m_Reg= nIndex == 1 ? "R310" : "R339";
            string booleanResult2m_Reg= nIndex == 1 ? "R311" : "R340";
            string cmdCalib4m_Reg = nIndex == 1 ? "R365" : "R384";
            string booleanResult4m_Reg = nIndex == 1 ? "R366" : "R385";



            STEP nStep = STEP.Wait_Calib_2m_Cmd;
            await Task.Run(() =>
            {
                int nCenterC1 = 0;
                int nCenterC2 = 0;

                switch (nStep)
                {
                    case STEP.Wait_Calib_2m_Cmd:
                        Thread.Sleep(100);
                        if (PLC.ReadInt(cmdCalib2m_Reg) == 1)
                            PopAndPushStep(STEP.Read_Center_Value_2m);
                        else if(PLC.ReadInt(cmdCalib2m_Reg) == 10)  //不做标定
                            PopAndPushStep(STEP.Finish_Calib);
                        break;

                    case STEP.Read_Center_Value_2m:
                        nCenterC1=lds.GetCenterValue(Prescription.CMosPointNumber);
                        PopAndPushStep(STEP.Write_Calib_2m_Boolean_Result);
                        break;
                    case STEP.Write_Calib_2m_Boolean_Result:
                        PLC.WriteInt(booleanResult2m_Reg, 2);   //2-ok,1-NG
                        PopAndPushStep(STEP.Finish_Calib_2m);
                        break;
                    case STEP.Finish_Calib_2m:
                        lds.HoldLDS();
                        PLC.WriteInt(cmdCalib2m_Reg, 2);
                        PopAndPushStep(STEP.Wait_Calib_4m_Cmd);
                        break;


                    case STEP.Wait_Calib_4m_Cmd:
                        Thread.Sleep(100);
                        if (PLC.ReadInt(cmdCalib4m_Reg) == 1)
                            PopAndPushStep(STEP.Read_Center_Value_4m);
                        break;
                    case STEP.Read_Center_Value_4m:
                        nCenterC2 = lds.GetCenterValue(Prescription.CMosPointNumber);
                        PopAndPushStep(STEP.Write_Calib_4m_Boolean_Result);
                        break;
                    case STEP.Write_Calib_4m_Boolean_Result:
                        PLC.WriteInt(booleanResult4m_Reg, 2);   //2-ok,1-NG
                        PopAndPushStep(STEP.Finish_Calib_4m);
                        break;
                    case STEP.Finish_Calib_4m:
                        PLC.WriteInt(cmdCalib4m_Reg, 2);
                        PopAndPushStep(STEP.Calclate_From_2m_4m);
                        break;
                    case STEP.Calclate_From_2m_4m:      //计算AB
                        if(lds.SetDataToLDS(nCenterC1, nCenterC2))
                            PopAndPushStep(STEP.Finish_Calib);
                        else
                            PopAndPushStep(STEP.Finish_With_Error);
                        break;
                    case STEP.Finish_With_Error:
                        PopAndPushStep(STEP.Finish_Calib);
                        break;
                    case STEP.Finish_Calib:
                        SetSubWorflowState(nIndex, true);
                        break;

                }
            });
        }
        private void SetSubWorflowState(int nIndex,bool bFinish)
        {
            int nState1 = nIndex == 1 ? 1 : 0;
            int nState2= nIndex == 1 ? 1 : 0;
            nSubWorkFlowState = nState1 + (nState2 << 1);
        }
        private bool GetSubWorkFlowState(int nIndex)
        {
            return 1 == ((nSubWorkFlowState >> (nIndex - 1)) & 0x01);
        }
    }
}
