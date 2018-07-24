
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
        private QSerisePlc PLC = null;
        private LDS lds1 = null;
        private LDS lds2 = null;
        public enum EnumCamID { Cam1, Cam2, Cam3, Cam4 };
        public enum EnumTarget { T2,T6}


        //Monitor
        private CancellationTokenSource ctsMonitorValue1 = null;
        private CancellationTokenSource ctsMonitorValue2 = null;
        private Task taskMonitorValue1 = null;
        private Task taskMonitorValue2 = null;
        private Dictionary<Int32, int> PosValueDic1 = new Dictionary<Int32, int>();
        private Dictionary<Int32, int> PosValueDic2 = new Dictionary<Int32, int>();
        private Task task1 = null, task2 = null;


        public WorkTune1(WorkFlowConfig cfg) : base(cfg)
        {

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
            lds1 = InstrumentMgr.Instance.FindInstrumentByName("LDS[2]") as LDS;
            lds2 = InstrumentMgr.Instance.FindInstrumentByName("LDS[3]") as LDS;
            #endregion

            bool bRet = PLC != null && lds1 != null && lds2 != null && Prescription != null;
            bRet = true;    //dddd
            if (!bRet)
                ShowInfo("初始化失败");
            else
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
            }
            return bRet;
        }

        protected override int WorkFlow()
        {
            return 0;
        }
        private void LdsWorkFunctionSet1()
        {
            int nIndex = 1;
            int nCmd = 0;
            const string cmdReg = "R107";

            const string Angle_Join_Reg = "R109";
            const string bool_Join_Reg = "R108";

            const string Angle_Blob_Reg = "R164";
            const string Bool_Blob_Reg = "R163";

            const string Intensity2m_Reg = "R171";
            const string bool_2m_Reg = "R173";

            const string Intensity6m_Reg = "R171";
            const string bool_6m_Reg = "R173";

            while (!cts.IsCancellationRequested)
            {
                bool bRet = false;
                nCmd = PLC.ReadInt(cmdReg);
                switch (nCmd)
                {
                    case 1: //计算对接角度
                        bRet = GetTune1JoinAngle(nIndex, out double Angle);
                        PLC.WriteDint(Angle_Join_Reg, /*Convert.ToInt32(Angle * 1000)*/123);
                        PLC.WriteInt(bool_Join_Reg, bRet ? 2 : 1);
                        PLC.WriteInt(cmdReg, nCmd + 1);
                        break;

                    case 3: //计算光斑角度
                        bRet = GetBlobAngle(nIndex, out double BlobAngle);
                        PLC.WriteDint(Angle_Blob_Reg, /*Convert.ToInt32(BlobAngle * 1000)*/11111111);
                        PLC.WriteInt(Bool_Blob_Reg, bRet ? 2 : 1);
                        PLC.WriteInt(cmdReg, nCmd + 1);
                        break;

                    case 5: //获取2米水平强度值
                        bRet = GetLaserIntensity(nIndex, EnumTarget.T2,out int InstensityValue2m);
                        PLC.WriteDint(Intensity2m_Reg, /*InstensityValue2m*/777777);
                        PLC.WriteInt(bool_2m_Reg, bRet ? 2 : 1);
                        PLC.WriteInt(cmdReg, nCmd + 1);
                        break;

                    case 7: //获取6米水平强度值
                        bRet = GetLaserIntensity(nIndex, EnumTarget.T6, out int InstensityValue6m);
                        PLC.WriteDint(Intensity6m_Reg, /*InstensityValue6m*/76);
                        PLC.WriteInt(bool_6m_Reg, bRet ? 2 : 1);
                        PLC.WriteInt(cmdReg, nCmd + 1);
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
        }
        private void LdsWorkFunctionSet2()
        {
            int nIndex = 2;
            int nCmd = 0;
            const string cmdReg = "R134";

            const string Angle_Join_Reg = "R136";
            const string bool_Join_Reg = "R135";

            const string Angle_Blob_Reg = "R182";
            const string Bool_Blob_Reg = "R181";

            const string Intensity2m_Reg = "R189";
            const string bool_2m_Reg = "R191";

            const string Intensity6m_Reg = "R189";
            const string bool_6m_Reg = "R191";

            while (!cts.IsCancellationRequested)
            {
                bool bRet = false;
                nCmd = PLC.ReadInt(cmdReg);
                switch (nCmd)
                {
                    case 1: //计算对接角度
                        bRet = GetTune1JoinAngle(nIndex, out double Angle);
                        PLC.WriteDint(Angle_Join_Reg, /*Convert.ToInt32(Angle * 1000)*/456);
                        PLC.WriteInt(bool_Join_Reg, bRet ? 2 : 1);
                        PLC.WriteInt(cmdReg, nCmd + 1);
                        break;

                    case 3: //计算光斑角度
                        bRet = GetBlobAngle(nIndex, out double BlobAngle);
                        PLC.WriteDint(Angle_Blob_Reg, /*Convert.ToInt32(BlobAngle * 1000)*/456456456);
                        PLC.WriteInt(Bool_Blob_Reg, bRet ? 2 : 1);
                        PLC.WriteInt(cmdReg, nCmd + 1);
                        break;

                    case 5: //获取2米水平强度值
                        bRet = GetLaserIntensity(nIndex, EnumTarget.T2, out int InstensityValue2m);
                        PLC.WriteDint(Intensity2m_Reg, /*InstensityValue2m*/77);
                        PLC.WriteInt(bool_2m_Reg, bRet ? 2 : 1);
                        PLC.WriteInt(cmdReg, nCmd + 1);
                        break;

                    case 7: //获取6米水平强度值
                        bRet = GetLaserIntensity(nIndex, EnumTarget.T6, out int InstensityValue6m);
                        PLC.WriteDint(Intensity6m_Reg, /*InstensityValue6m*/66);
                        PLC.WriteInt(bool_6m_Reg, bRet ? 2 : 1);
                        PLC.WriteInt(cmdReg, nCmd + 1);
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
        }

        private bool GetTune1JoinAngle(int nIndex, out double Angle)
        {
            bool bRet = false;
            Angle = 0;
            return true;
            if (nIndex != 1 && nIndex != 2)
                return false;

            int nCamID = nIndex == 1 ? 0 : 1;
            bRet=Vision.Vision.Instance.ProcessImage(Vision.Vision.IMAGEPROCESS_STEP.GET_ANGLE_TUNE1, nCamID, null,out object result);
            if (bRet)
                Angle = double.Parse(result.ToString());
            return bRet;
        }
        private bool GetBlobAngle(int nIndex, out double Angle)
        {
            Angle = 0;
            return true;
            if (nIndex != 1 && nIndex != 2)
                return false;
            bool bRet = false;
            int nCamID = nIndex == 1 ? 0 : 1;
            bRet=Vision.Vision.Instance.ProcessImage(Vision.Vision.IMAGEPROCESS_STEP.GET_ANGLE_BLOB, nCamID, null, out object result);
            if (bRet)
                Angle = double.Parse(result.ToString());
            return bRet;
        }
        private bool GetLaserIntensity(int nIndex, EnumTarget target, out int InstensityValue)
        {
            InstensityValue = 0;
            return true;
            if (nIndex != 1 && nIndex != 2)
                return false;
            LDS lds= nIndex == 1 ? lds1 : lds2;
            InstensityValue = lds.GetExposeValue(Prescription.CMosPointNumber);
            return InstensityValue > 0; ;
        }
        private bool ReadResutFromPLC(int nIndex)
        {
            return true;
        }
    }
}
