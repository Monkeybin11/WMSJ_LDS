using CPAS.Config;
using CPAS.Config.HardwareManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using T3Unload;
namespace CPAS.Instrument
{
    public class QSerisePlc : InstrumentBase
    {
        private ComportCfg comportCfg = null;
        private EtherNetCfg etherNetCfg = null;
        private PLC_MS plc = new PLC_MS();
        public QSerisePlc(HardwareCfgLevelManager1 cfg) : base(cfg) { }
        public override bool DeInit()
        {
            if (plc != null)
                return 0 == plc.ClosePLC();
            return true;
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
                    else
                        return false;
                }
                else if (Config.ConnectMode.ToUpper() == @"ETHERNET")
                {
                    foreach (var it in hardwareCfg.EtherNets)
                    {
                        if (it.PortName == Config.PortName)
                        {
                            etherNetCfg = it;
                            break;
                        }
                    }
                    if (etherNetCfg == null)
                        return false;
                    int nRet = plc.InitPLC(etherNetCfg.Port.ToString());
                    return nRet == 0;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public Int32 ReadDint(string strRegisterName)
        {
            strRegisterName = strRegisterName.ToUpper();
            lock (_lock)
            {
                if (plc != null)
                {
                    strRegisterName = strRegisterName.ToUpper();
                    string[] outValueList = new string[] { };
                    string strType = strRegisterName.Substring(0, 1);
                    int nAddress = Convert.ToInt16(strRegisterName.Substring(1, strRegisterName.Length - 1));
                    plc.RaedPLCRandom(new string[] { strRegisterName, strType + (nAddress + 1).ToString() }, "2", out outValueList);
                    string strValue = string.Format("{0:X4}{1:X4}", Int16.Parse(outValueList[1]), Int16.Parse(outValueList[0]));
                    return Convert.ToInt32(strValue, 16);
                }
                else
                {
                    return 0;
                }
            }
        }
        public bool WriteDint(string strRegisterName, Int32 nValue)
        {
            strRegisterName = strRegisterName.ToUpper();
            lock (_lock)
            {
                if (plc != null)
                {
                    strRegisterName = strRegisterName.ToUpper();
                    string strType = strRegisterName.Substring(0, 1);
                    int nAddress = Convert.ToInt16(strRegisterName.Substring(1, strRegisterName.Length - 1));
                    string strValue = string.Format("{0:X8}", nValue);
                    return 0 == plc.WriteDeviceRandom(new string[] { strRegisterName, strType + (nAddress + 1).ToString() }, "2", new string[] { Convert.ToInt16(strValue.Substring(4, 4), 16).ToString(), Convert.ToInt16((strValue.Substring(0, 4)), 16).ToString() });
                }
                return false;
            }
        }
        public Int16 ReadInt(string strRegisterName)
        {
            strRegisterName = strRegisterName.ToUpper();
            lock (_lock)
            {
                strRegisterName = strRegisterName.ToUpper();
                string[] outValueList = new string[] { };
                string strType = strRegisterName.Substring(0, 1);
                int nAddress = Convert.ToInt16(strRegisterName.Substring(1, strRegisterName.Length - 1));
                plc.RaedPLCRandom(new string[] { strRegisterName }, "1", out outValueList);
                string strValue = string.Format("{0:X4}", Int16.Parse(outValueList[0]));
                return Convert.ToInt16(strValue, 16);
            }
        }
        public bool WriteInt(string strRegisterName, int nValue)
        {
            strRegisterName = strRegisterName.ToUpper();
            lock (_lock)
            {
                strRegisterName = strRegisterName.ToUpper();
                string strValue = string.Format("{0:X4}", nValue);
                return 0 == plc.WriteDeviceRandom(new string[] { strRegisterName }, "1", new string[] { Convert.ToInt16(strValue, 16).ToString() });
            }
        }
        public bool WriteString(string strRegisterName, string str)
        {
            bool bRet = true;
            strRegisterName = strRegisterName.ToUpper();
            string strType = strRegisterName.Substring(0, 1);
            int nRegisterStart = Convert.ToInt16(strRegisterName.Substring(1, strRegisterName.Length - 1));
            string strRealRegister = "";
            string strValue = "";
            StringBuilder sb = new StringBuilder();
            bool bEven = str.Length % 2 == 0;
            int len = bEven ? str.Length / 2 : str.Length / 2 + 1; 
            int i;
            for (i = 0; i < (bEven? len : len -1); i++)
            {
 
                strValue = string.Format("{0:X2}{1:X2}", (byte)str[i * 2], (byte)str[i * 2 + 1]);
                strRealRegister = strType + nRegisterStart.ToString();
                bRet &= WriteInt(strRealRegister, Convert.ToInt16(strValue.ToString(), 16));             
                nRegisterStart++;
            }
            if (!bEven)
            {
                strValue = string.Format("{0:X2}00", (byte)str[i * 2]);
                strRealRegister = strType + nRegisterStart.ToString();
                bRet &= WriteInt(strRealRegister, Convert.ToInt16(strValue.ToString(), 16));
            }
            return true;
        }
        public string ReadString(string strRegisterName, int nLength)
        {
            strRegisterName = strRegisterName.ToUpper();
            string strType = strRegisterName.Substring(0, 1);
            string strRealRigster = "";
            int nRegisterStart = Convert.ToInt16(strRegisterName.Substring(1, strRegisterName.Length - 1));
            List<byte> byteList = new List<byte>();


            for (int i = 0; i < nLength / 2; i++)
            {
                strRealRigster = string.Format("{0}{1}", strType, nRegisterStart);
                string strRet = string.Format("{0:X4}", ReadInt(strRealRigster));
                byteList.Add(Convert.ToByte(strRet.Substring(0, 2), 16));
                byteList.Add(Convert.ToByte(strRet.Substring(2, 2), 16));
                nRegisterStart++;
            }

            if (nLength % 2 == 1)    //判断是否有最后一个字符
            {
                strRealRigster = string.Format("{0}{1}", strType, nRegisterStart);
                string strRet = string.Format("{0:X2}", ReadInt(strRealRigster));
                byteList.Add(Convert.ToByte(strRet.Substring(0, 2), 16));
            }
            return System.Text.Encoding.ASCII.GetString(byteList.ToArray());
        }
    }
}
