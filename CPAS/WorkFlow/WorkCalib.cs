
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

        private Task task1 = null, task2 = null;

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
            bRet = true;    //dddd
            if (!bRet)
                ShowInfo("初始化失败");
            else
            {
                
            }
            return bRet;
        }
        public WorkCalib(WorkFlowConfig cfg) : base(cfg)
        {
        }
        protected override int WorkFlow()
        {
            if (task1 == null || task1.IsCanceled || task1.IsCompleted)
            {
                task1 = new Task(() =>
                {
                    LdsWorkFunctionSet1();
                });
                task1.Start();
            }
            if (task2 == null || task2.IsCanceled || task2.IsCompleted)
            {
                task2 = new Task(() =>
                {
                    LdsWorkFunctionSet2();
                });
                task2.Start();
            }
            return 0;
        }
        private void LdsWorkFunctionSet1()
        {
            int nIndex = 1;
            int nCmd = 0;
            const string cmdReg = "R310";

            int C1 = 0;
            int C2 = 0;

            const string bool_Calib2m = "R311";
            const string bool_Calib4m = "R366";
            try
            {
                while (!cts.IsCancellationRequested)
            {
               
                    bool bRet = false;
                    nCmd = PLC.ReadInt(cmdReg);
                    switch (nCmd)
                    {
                        case 1: //读取2米强度值
                            bRet = GetCenterFocus(nIndex, out C1);
                            PLC.WriteInt(bool_Calib2m, bRet ? 2 : 1);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            ShowInfo("读取LDS1的2米处激光中心值");
                            break;

                        case 3: //读取4米强度值
                            bRet = GetCenterFocus(nIndex, out C1);
                            PLC.WriteInt(bool_Calib4m, bRet ? 2 : 1);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            ShowInfo("读取LDS1的4米处激光中心值");
                            break;

                        case 5: //计算结果给LDS
                            bRet = SetLDSCalidData(nIndex, C1, C2);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            ShowInfo("计算结果写入LDS1");
                            //结果写在哪
                            break;
                        case 100:
                            ReadResutFromPLC(nIndex);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            break;
                        default:
                            break;
                    }
                    Thread.Sleep(100);
                }
                ShowInfo("LDS1的服务器被用户退出");
            }
            catch (Exception ex)
            {
                ShowInfo($"LDS1的服务器异常终止:{ex.Message}");
            }


        }
        private void LdsWorkFunctionSet2()
        {
            int nIndex = 2;
            int nCmd = 0;
            const string cmdReg = "R339";

            int C1 = 0;
            int C2 = 0;

            const string bool_Calib2m = "R340";
            const string bool_Calib4m = "R385";
            const string bool_CalibFinalResult = "R385";
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    bool bRet = false;
                    nCmd = PLC.ReadInt(cmdReg);
                    switch (nCmd)
                    {
                        case 1: //读取2米强度值
                            bRet = GetCenterFocus(nIndex, out C1);
                            PLC.WriteInt(bool_Calib2m, bRet ? 2 : 1);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            ShowInfo("读取LDS2的2米中心值");
                            break;

                        case 3: //读取4米强度值
                            bRet = GetCenterFocus(nIndex, out C1);
                            PLC.WriteInt(bool_Calib4m, bRet ? 2 : 1);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            ShowInfo("读取LDS2的4米中心值");
                            break;

                        case 5: //计算结果给LDS
                            bRet = SetLDSCalidData(nIndex, C1, C2);
                            PLC.WriteInt(bool_CalibFinalResult, bRet ? 2 : 1);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            ShowInfo("计算结果写入LDS2");
                            break;


                        case 100:
                            ReadResutFromPLC(nIndex);
                            PLC.WriteInt(cmdReg, nCmd + 1);
                            break;
                        default:
                            break;
                    }
                    Thread.Sleep(100);
                }
                ShowInfo("LDS1的服务器被用户退出");
            }
            catch (Exception ex)
            {
                ShowInfo($"LDS1的服务器异常终止:{ex.Message}");
            }
        }


        private bool GetCenterFocus(int nIndex, out int CenterValue)
        {
            CenterValue = 0;
            return true;
            bool bRet = false;
            if (nIndex < 1 || nIndex > 2)
                return false;
            LDS lds = nIndex == 1 ? lds1 : lds2;
            CenterValue=lds.GetFocusValue(Prescription.CMosPointNumber);
            bRet = CenterValue >= 0;
            return bRet;
        }
        private bool SetLDSCalidData(int nIndex,int c1,int c2)
        {
            return true;
            if (nIndex < 1 || nIndex > 2)
                return false;
            LDS lds = nIndex == 1 ? lds1 : lds2;
            return lds.SetDataToLDS(c1, c2);
        }
        private bool ReadResutFromPLC(int nIndex)
        {
            return true;
        }
    }
}
