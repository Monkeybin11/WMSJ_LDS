using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using CPAS.Config.HardwareManager;
using CPAS.Config;


namespace CPAS.Instrument
{
    public class LDS : InstrumentBase
    {
        private byte[] byteRecv = new byte[1024*2];
        private ComportCfg comportCfg = null;
        public  LDS(HardwareCfgLevelManager1 cfg) : base(cfg)
        { 

        }

        public override bool Init()
        {
            try
            {
                HardwareCfgManager hardwareCfg = ConfigMgr.HardwareCfgMgr;
                if (Config.ConnectMode.ToUpper() == @"COMPORT")
                {
                    foreach (var it in hardwareCfg.Comports)
                    {
                        if (it.PortName == Config.PortName)
                            comportCfg = it;
                    }
                    comPort = new System.IO.Ports.SerialPort();
                    if (comPort != null && comportCfg != null)
                    {
                        GetPortProfileData(comportCfg);
                        comPort.PortName = comportData.Port;
                        comPort.BaudRate = comportData.BaudRate;
                        comPort.Parity = comportData.parity;
                        comPort.StopBits = comportData.stopbits;
                        comPort.DataBits = comportData.DataBits;
                        comPort.ReadTimeout = comportData.Timeout;
                        comPort.WriteTimeout = comportData.Timeout;
                        if (comPort.IsOpen)
                            comPort.Close();
                        comPort.Open();
                        return comPort.IsOpen;
                    }
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public override bool DeInit()
        {
            if (comPort != null)
            {
                comPort.Close();
                comPort.Dispose();
            }
            return true;
        }

        #region 设置功率
        public bool CreasePower(bool bCrease)
        { 
            string strCmd=string.Format("laserpower{0}$",bCrease?"add" : "sub");
            lock (comPort)
            {
                if (comPort == null)
                    return false;
                comPort.Write(strCmd);
                return true;
            }
        }
        public double GetCurLaserPowerValue()
        {
            lock (comPort)
            {
                if (comPort == null)
                    return 0.0f;
                comPort.Write("laserpowerok$");
                return 0.0f;
            }
        }
        public bool CheckStatusOK()
        {
            lock (comPort)
            {
                if (comPort == null)
                    return false;
                comPort.Write("getstatuscode$");
                return true;
            }
        }
        #endregion

        #region 烧录
        /// <summary>
        /// 准备烧录
        /// </summary>
        /// <returns></returns>
        private bool PrepareRecord()
        {
            if (comPort == null || !comPort.IsOpen)
                return false;
            lock (comPort)
            {
                comPort.Write("flashserial$");
                return true;
            }
        }
        private bool DoRecord(string strID)
        {
            if (comPort == null || !comPort.IsOpen)
                return false;
            lock (comPort)
            {
                comPort.Write(string.Format("Manualfacture[{0}]$", strID));
                return true;
            }
        }
        private bool CheckStatus()
        {
            if (comPort == null || !comPort.IsOpen)
                return false;
            lock (comPort)
            {
                comPort.Write("getstatuscode$");     //code?
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetSerial()
        {
            if (comPort == null || !comPort.IsOpen)
                return "";
            lock (comPort)
            {
                comPort.Write("getserial$");
                Thread.Sleep(50);
                comPort.Read(byteRecv, 0, 64);
                return System.Text.Encoding.ASCII.GetString(byteRecv);
            }
        }
        /// <summary>
        /// 烧录流程
        /// </summary>
        /// <param name="strID"></param>
        /// <returns></returns>
        public bool RecodIDFlow(string strID)   
        {
            PrepareRecord();
            DoRecord(strID);
            CheckStatus();
            return strID == GetSerial();
        }
        #endregion

        #region 调水平
        /// <summary>
        /// 读取曝光值
        /// </summary>
        /// <returns></returns>
        public int GetExposeValue()
        {
            if (comPort == null || !comPort.IsOpen)
                return 0;
            lock (comPort)
            {
                comPort.Write("sethorizontal$");
                Thread.Sleep(50);
                comPort.Read(byteRecv, 0, 64);
                if (byteRecv[0] == 0x5a && byteRecv[1] == 0xa5 && byteRecv[2] == 0x5a && byteRecv[3] == 0xa5)
                    return byteRecv[4] + byteRecv[5] << 8;
                return 0;
            }
        }
        /// <summary>
        /// 确定调整结果
        /// </summary>
        public void HoldLDS()
        {
            if (comPort == null || !comPort.IsOpen)
                return;
            lock (comPort)
            {
                comPort.Write("holdlds$");
            }
        }

        #endregion

        #region 调焦距
        public int GetFocusValue()
        {
            if (comPort == null || !comPort.IsOpen)
                return 0;
            lock (comPort)
            {
                comPort.Write("focuslds$");
                Thread.Sleep(50);
                comPort.Read(byteRecv, 0, 64);
                if (byteRecv[0] == 0x5a && byteRecv[1] == 0xa5 && byteRecv[2] == 0x5a && byteRecv[3] == 0xa5)
                    return byteRecv[4] + byteRecv[5] << 8;
                return 0;
            }
        }
        #endregion

        #region 距离标定
        /// <summary>
        /// 得到中心值，不能发送太快
        /// </summary>
        /// <returns></returns>
        public int GetCenterValue()
        {
            if (comPort == null || !comPort.IsOpen)
                return 0;
            lock (comPort)
            {
                comPort.Write("sendpeakpos$");
                Thread.Sleep(1500);
                comPort.Read(byteRecv, 0, 64);
                if (byteRecv[0] == 0xfa)
                    return byteRecv[1] + byteRecv[2] << 8+ byteRecv[3]<<16;
                return 0;
            }
        }

        /// <summary>
        /// 标定
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public bool SetDataToLDS(int c1, int c2)
        {   
            if (comPort == null || !comPort.IsOpen)
            return false;
            lock (comPort)
            {
                UInt32 B = (UInt32)((1100 * c1 - 22000 * c2) / 2000);
                UInt32 A = (UInt32)(2000 * B + 11000 * c1);
                comPort.Write("flashlds$");
                Thread.Sleep(50);
                byte[] byteSend = new byte[9];
                byteSend[8] = (byte)'$';
                for (int i=0;i<4;i++)
                    byteSend[i] = (byte)((A >> 24-8*i) & 0xFF);
                for (int i = 4; i < 8; i++)
                    byteSend[i] = (byte)((B >> 24 - 8 * (i-4)) & 0xFF);
                comPort.Write(byteSend, 0, byteSend.Length);
                Thread.Sleep(50);
                comPort.Write("getstatuscode$");     //注意这个返回值还没有判断
                Thread.Sleep(50);
                comPort.Write("holdlds$");       //注意这个返回值还没有判断
                return true;
            }
        }

     
        #endregion

    }
}
