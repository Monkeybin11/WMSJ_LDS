using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
namespace CPAS.Classes
{
    
    public class LDS
    {
        private byte[] byteRecv = new byte[64];
        private SerialPort sp = null;
        private LDS() { }
        public LDS(SerialPort sp)
        {
            this.sp = sp;
        }
        public bool Init()
        {
            if (sp == null)
                return false;
            if (sp.IsOpen)
                sp.Close();
            sp.Open();
            return sp.IsOpen;
        }
        #region 设置功率
        public bool CreasePower(bool bCrease)
        { 
            string strCmd=string.Format("laserpower{0}$",bCrease?"add" : "sub");
            lock (sp)
            {
                if (sp == null)
                    return false;
                sp.Write(strCmd);
                return true;
            }
        }
        public double GetCurLaserPowerValue()
        {
            lock (sp)
            {
                if (sp == null)
                    return 0.0f;
                sp.Write("laserpowerok$");
                return 0.0f;
            }
        }
        public bool CheckStatusOK()
        {
            lock (sp)
            {
                if (sp == null)
                    return false;
                sp.Write("getstatuscode$");
                return true;
            }
        }
        #endregion

        #region 烧录
        private bool PrepareRecord()
        {
            if (sp == null || !sp.IsOpen)
                return false;
            lock (sp)
            {
                sp.Write("flashserial$");
                return true;
            }
        }
        private bool DoRecord(string strID)
        {
            if (sp == null || !sp.IsOpen)
                return false;
            lock (sp)
            {
                sp.Write(string.Format("Manualfacture[{0}]$", strID));
                return true;
            }
        }
        private bool CheckStatus()
        {
            if (sp == null || !sp.IsOpen)
                return false;
            lock (sp)
            {
                sp.Write("getstatuscode$");     //code?
                return true;
            }
        }
        private string GetSerial()
        {
            if (sp == null || !sp.IsOpen)
                return "";
            lock (sp)
            {
                sp.Write("getserial$");
                Thread.Sleep(50);
                sp.Read(byteRecv, 0, 64);
                return System.Text.Encoding.ASCII.GetString(byteRecv);
            }
        }
        public bool RecodIDFlow(string strID)
        {
            PrepareRecord();
            DoRecord(strID);
            CheckStatus();
            return strID == GetSerial();
        }
        #endregion

        #region 调水平
        public int GetExposeValue()
        {
            if (sp == null || !sp.IsOpen)
                return 0;
            lock (sp)
            {
                sp.Write("sethorizontal$");
                Thread.Sleep(50);
                sp.Read(byteRecv, 0, 64);
                if (byteRecv[0] == 0x5a && byteRecv[1] == 0xa5 && byteRecv[2] == 0x5a && byteRecv[3] == 0xa5)
                    return byteRecv[4] + byteRecv[5] << 8;
                return 0;
            }
        }
        public void HoldLDS()
        {
            if (sp == null || !sp.IsOpen)
                return;
            lock (sp)
            {
                sp.Write("holdlds$");
            }
        }

        #endregion

        #region 调焦距
        public int GetFocusValue()
        {
            if (sp == null || !sp.IsOpen)
                return 0;
            lock (sp)
            {
                sp.Write("focuslds$");
                Thread.Sleep(50);
                sp.Read(byteRecv, 0, 64);
                if (byteRecv[0] == 0x5a && byteRecv[1] == 0xa5 && byteRecv[2] == 0x5a && byteRecv[3] == 0xa5)
                    return byteRecv[4] + byteRecv[5] << 8;
                return 0;
            }
        }
        #endregion

        #region 距离标定
        public int GetCenterValue()
        {
            if (sp == null || !sp.IsOpen)
                return 0;
            lock (sp)
            {
                sp.Write("sendpeakpos$");
                Thread.Sleep(1500);
                sp.Read(byteRecv, 0, 64);
                if (byteRecv[0] == 0xfa)
                    return byteRecv[1] + byteRecv[2] << 8+ byteRecv[3]<<16;
                return 0;
            }
        }

        public bool SetDataToLDS(int c1, int c2)
        {   
            if (sp == null || !sp.IsOpen)
            return false;
            lock (sp)
            {
                UInt32 B = (UInt32)((1100 * c1 - 22000 * c2) / 2000);
                UInt32 A = (UInt32)(2000 * B + 11000 * c1);
                sp.Write("flashlds$");
                Thread.Sleep(50);
                byte[] byteSend = new byte[9];
                byteSend[8] = (byte)'$';
                for (int i=0;i<4;i++)
                    byteSend[i] = (byte)((A >> 24-8*i) & 0xFF);
                for (int i = 4; i < 8; i++)
                    byteSend[i] = (byte)((B >> 24 - 8 * (i-4)) & 0xFF);
                sp.Write(byteSend, 0, byteSend.Length);
                Thread.Sleep(50);
                sp.Write("getstatuscode$");     //注意这个返回值还没有判断
                Thread.Sleep(50);
                sp.Write("holdlds$");
                return true;
            }
        }
        #endregion

    }
}
