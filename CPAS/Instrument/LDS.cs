using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using CPAS.Config.HardwareManager;
using CPAS.Config;
using CPAS.Models;
using System.Runtime.InteropServices;

namespace CPAS.Instrument
{
    public class LDS : InstrumentBase
    {
        //private byte[] byteRecv = new byte[1024 * 2];
        private ComportCfg comportCfg = null;
        public LDS(HardwareCfgLevelManager1 cfg) : base(cfg)
        {
            LDSResult = new LDSResultModel();
        }
        public LDSResultModel LDSResult{get;set;}
        private byte[] ldsHeader = new byte[] { 0x5a, 0xa5, 0x5a, 0xa5 };
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
                        {
                            comportCfg = it;
                            break;
                        }
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
                        comPort.ReadBufferSize = 6000;  //6000个字节
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
        public bool MyInit(ComportCfg comportCfg)
        {
            try
            {
                comPort = new System.IO.Ports.SerialPort();
                if (comPort != null && comportCfg != null)
                {
                    this.comportCfg = comportCfg;
                    GetPortProfileData(comportCfg);
                    comPort.PortName = comportData.Port;
                    comPort.BaudRate = comportData.BaudRate;
                    comPort.Parity = comportData.parity;
                    comPort.StopBits = comportData.stopbits;
                    comPort.DataBits = comportData.DataBits;
                    comPort.ReadTimeout = comportData.Timeout;
                    comPort.WriteTimeout = comportData.Timeout;
                    comPort.ReadBufferSize = 4000;  //4000个字节
                    if (comPort.IsOpen)
                        comPort.Close();
                    comPort.Open();
                    return comPort.IsOpen;
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
        public bool InCreasePower(bool bCrease)
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
        public void EnsureLaserPower()
        {
            lock (comPort)
            {
                if (comPort == null)
                    return ;
                comPort.Write("laserpowerok$");
            }
        }
        public bool CheckSetPowerStatusOK()
        {
            lock (comPort)
            {
                if (comPort == null)
                    return false;
                comPort.Write("getstatuscode$");
                Thread.Sleep(20);
                byte[] recv = new byte[10];
                comPort.Read(recv, 0, 10);
                string strRet = System.Text.Encoding.UTF8.GetString(recv);
                return strRet=="BR";
            }
        }
        #endregion


        #region 烧录 SN
        public bool DoRecord(string strID)
        {
            if (comPort == null || !comPort.IsOpen)
                return false;
            lock (comPort)
            {
                comPort.Write("flashserial$");
                Thread.Sleep(50);
                comPort.Write(string.Format("{0}$", strID));
                Thread.Sleep(50);
                comPort.Write("getserial$");
                Thread.Sleep(50);
                byte[] recv = new byte[30];
                comPort.Read(recv, 0, 30);
                string strRet = System.Text.Encoding.UTF8.GetString(recv).Replace("\0","");
                return strRet==strID;
            }
        }
 
        #endregion

        #region 调水平
        /// <summary>
        /// 读取曝光值
        /// </summary>
        /// <returns></returns>
        public int GetExposeValue(int nCmosLength)  //最大值-底噪
        {
            if (comPort == null || !comPort.IsOpen)
                return -1;
            lock (comPort)
            {
                comPort.Write("sethorizontal$");
                Thread.Sleep(400);
                byte[] recv = new byte[4000];
                comPort.Write("holdlds$");
                Thread.Sleep(20);
                comPort.Read(recv, 0, 4000);
                int[] posArr=SearchHeader(recv, ldsHeader, nCmosLength*2);
                if (posArr.Length >= 1)      
                {
                    var finaList = recv.Skip(posArr[0]+ldsHeader.Length).Take(nCmosLength * 2);      //一帧数据
                    int[] intArr=ByteArr2IntArr(finaList);
                    int sum = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        sum += intArr[i];
                        sum += intArr[intArr.Length - i - 1];
                    }
                    int meanValue = sum / 20;   //底噪
                    int value=intArr.Skip(10).Take(intArr.Length - 20).Max()-meanValue;
                    Console.WriteLine("强度值:"+value.ToString());
                    return value;
                }
                else
                    return -1;
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
        public int GetFocusValue(int nCmosLength)
        {
            if (comPort == null || !comPort.IsOpen)
                return 0;
            lock (comPort)
            {
                comPort.Write("sethorizontal$");
                Thread.Sleep(100);
                byte[] recv = new byte[6000];
                comPort.Write("holdlds$");
                Thread.Sleep(20);
                comPort.Read(recv, 0, 6000);
                int[] posArr = SearchHeader(recv, ldsHeader, nCmosLength * 2);
                if (posArr.Length > 1)
                {
                    var finaList = recv.Skip(posArr[0] + ldsHeader.Length).Take(nCmosLength * 2);      //一帧数据
                    int[] intArr = ByteArr2IntArr(finaList);
                    int sum = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        sum += intArr[i];
                        sum += intArr[intArr.Length - i - 1];
                    }
                    int meanValue = sum / 20;   //底噪
                    int value = intArr.Skip(10).Take(intArr.Length - 20).Max() - meanValue;
                    return value;
                }
                else
                    return 0;
            }
        }
        #endregion

        #region 距离标定
        /// <summary>
        /// 得到中心值，不能发送太快
        /// </summary>
        /// <returns></returns>
        public int GetCenterValue(int nCmosLength)      //中心值需要减去底噪吗
        {
            if (comPort == null || !comPort.IsOpen)
                return 0;
            lock (comPort)
            {
                comPort.Write("sethorizontal$");
                Thread.Sleep(100);
                byte[] recv = new byte[6000];
                comPort.Write("holdlds$");
                Thread.Sleep(20);
                comPort.Read(recv, 0, 6000);
                int[] posArr = SearchHeader(recv, ldsHeader, nCmosLength * 2);
                if (posArr.Length > 1)
                {
                    var finaList = recv.Skip(posArr[0] + ldsHeader.Length).Take(nCmosLength * 2);      //一帧数据
                    int[] intArr = ByteArr2IntArr(finaList);
                    int value = nCmosLength % 2 == 0 ? (intArr[nCmosLength / 2] + intArr[nCmosLength / 2 + 1]) / 2 : intArr[nCmosLength / 2];
                    return value;
                }
                else
                    return 0;
            }
        }

 
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
        public bool LdsUnLock(out string strWhy)
        {
            strWhy = "";
            string s1 = "aa";
            string s2 = "bb";
            IntPtr[] pts = new IntPtr[1];
            pts[0] = Marshal.StringToHGlobalAnsi(s1);
            bool bRet=LdsUnLock(Convert.ToInt16(comportCfg.Port.Replace("COM","")), pts);
            String a = Marshal.PtrToStringAnsi(pts[0]);
            strWhy = a;
            return bRet;
        }

        #endregion

        public int[] SearchHeader(byte[] arr, byte[] header)
        {
            int nLenArr = arr.Length;
            int nLenHeader = header.Length;

            List<int> ls = new List<int>();
            List<int> ll = new List<int>();
            for (int i = 0; i < nLenArr; i++)
            {
                if (arr[i] == header[0])
                {
                    ls.Add(i);
                }
            }
            foreach (var it in ls)
            {
                if (it < nLenArr - nLenHeader)
                {
                    bool b = true;
                    for (int i = 0; i < nLenHeader; i++)
                    {
                        b &= arr[it + i] == header[i];
                    }
                    if (b)
                        ll.Add(it);
                }
            }
            return ll.ToArray();
        }
        public int[] SearchHeader(byte[] arr, byte[] header, int subArrLen)
        {
            int nLenArr = arr.Length;
            int nLenHeader = header.Length;

            List<int> ls = new List<int>();
            List<int> ll = new List<int>();
            List<int> y = new List<int>();
            for (int i = 0; i < nLenArr; i++)
            {
                if (arr[i] == header[0])
                {
                    ls.Add(i);
                }
            }
            foreach (var it in ls)
            {
                if (it < nLenArr - nLenHeader)
                {
                    bool b = true;
                    for (int i = 0; i < nLenHeader; i++)
                    {
                        b &= arr[it + i] == header[i];
                    }
                    if (b)
                        ll.Add(it);
                }
            }
            for (int i = 0; i < ll.Count() - 1; i++)
            {
                if (ll[i + 1] - ll[i] == subArrLen + nLenHeader)
                    y.Add(ll[i]);
            }
            return y.ToArray();
        }
        public int[] ByteArr2IntArr(byte[] sourceArr)
        {
            List<int> intRawData = new List<int>();
            int nLength = sourceArr.Length;
            if (nLength % 2 != 0)
                throw new Exception("Wrong length when convert byteArr to intArr");
            else
            {
                for (int i = 0; i < nLength/2;i++)
                {
                    intRawData.Add(sourceArr[2 * i] + sourceArr[2 * i + 1] << 8);
                }
            }
            return intRawData.ToArray();
        }
        public int[] ByteArr2IntArr(IEnumerable<byte> sourceArr)
        {
            List<int> intRawData = new List<int>();
            int nLength = sourceArr.Count();
            if (nLength % 2 != 0)
                throw new Exception("Wrong length when convert byteArr to intArr");
            else
            {
                for (int i = 0; i < nLength / 2; i++)
                {
                    intRawData.Add(sourceArr.ElementAt(i*2) + sourceArr.ElementAt(2 * i + 1) << 8);
                }
            }
            return intRawData.ToArray();
        }

        [DllImport("LdsUnlockLibrary.dll", EntryPoint = "?LdsUnlock@@YA_NHPAPAD@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LdsUnLock(int nPortNum, IntPtr[] ptrs);
    }
}
