using CPAS.Config;
using CPAS.Config.HardwareManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPAS.Instrument
{
    public class QSerisePlc : InstrumentBase
    {
        private ComportCfg comportCfg = null;
        private EtherNetCfg etherNetCfg = null;
        private string StrProtocolHeader = "50,00,00,FF,FF,03,00,DataLength,10,00,";
        public QSerisePlc(HardwareCfgLevelManager1 cfg) : base(cfg) { }

        public override bool DeInit()
        {
            if (EtherNetPort != null)
                EtherNetPort.Close();
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
                    else if (Config.ConnectMode.ToUpper() == @"ETHERNET")
                    {
                        foreach (var it in hardwareCfg.EtherNets)
                        {
                            if (it.PortName == Config.PortName)
                            {
                                etherNetCfg = it;
                                EtherNetPort = new Communication.EtherNet(0, it.PortName, it.IP, it.Port, it.TimeOut, "", "");
                            }
                        }
                        if (etherNetCfg == null || EtherNetPort==null)
                            return false;

                        EtherNetPort.Open();
                        return EtherNetPort.IsConnection;
                    }
                    else
                        return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool ReadDint(string strRegisterName, out Int32 nValue)
        {
            try
            {
                lock (_lock)
                {
                    nValue = 0;
                    byte[] byteRecv = new byte[100];
                    ConvertName2Hex(strRegisterName, out string strRegisterAddress);
                    if (EtherNetPort != null && EtherNetPort.IsConnection)
                    {
                        string strCmd = "01,04,00,00,";    //读命令
                        string strWordNum = string.Format("{0:X},{1:X}", 2 & 0xFF, (2 >> 16) & 0xFF); //2个字
                        string strHeader = StrProtocolHeader;
                        strHeader.Replace("DataLength", "0C,00");
                        string strSend = strHeader + strCmd + strRegisterAddress + strWordNum;
                        string[] SplitStringList = strSend.Split(',');
                        int len = SplitStringList.Length;
                        List<byte> dataSendList = new List<byte>();
                        foreach (var it in SplitStringList)
                            dataSendList.Add(Convert.ToByte(it, 16));


                        if (EtherNetPort.WriteData(dataSendList.ToArray(), dataSendList.Count))
                        {
                            Thread.Sleep(50);  //等待100ms再去读取返回值
                            EtherNetPort.ReadData(byteRecv, 100); //检查读取到的数据
                            if (CheckRecvData(byteRecv))//结束代码是0就是成功
                            {
                                //for (int i = 0; i < 2; i++)  //nNum个字元件
                                //    iArray[i] = (int)byteRecv[i * 2 + 11] + (int)(byteRecv[i * 2 + 12] << 16);//L,H

                            }
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                nValue = 0;
                return false;
            }
    
        }
        public bool WriteDint(string strRegisterName,out Int32 nValue)
        {
            try
            {
                lock (_lock)
                {
                    nValue = 0;
                    bool bRet = false;
                    string strCmd = "01,14,00,00,";  //写命令
                    byte[] byteRecv = new byte[100];
                    ConvertName2Hex(strRegisterName, out string strRegisterAddress);  //元件代码以及其实地址
                                                                                      //写入的软元件个数(word为单位)
                    string strNum = string.Format("{0:X},{0:X},", 2 & 0xFF, (2 >> 8) & 0xFF);
                    //替换发送数据的长度
                    int dataLen = 12 + 2 * 2;
                    string strLen = string.Format("{0:X},{0:X}", dataLen & 0xFF, (dataLen >> 8) & 0xFF);
                    string strHeader = StrProtocolHeader;
                    strHeader.Replace("DataLength", strLen);

                    string strData = "", strTemp = "";
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 16; j++)
                        {
                            strTemp = string.Format("{0:X},{0:X}", nValue & 0xFF, (nValue >> 8) & 0xFF);//低8位，高8位
                            if (i < 2 - 1)
                                strTemp += ",";
                        }
                        //strData += strTemp;????
                    }
                    string strSend = strHeader + strCmd + strRegisterAddress + strNum + strData;
                    string[] SplitStringList = strSend.Split(',');
                    int len = SplitStringList.Length;
                    List<byte> dataSendList = new List<byte>();
                    foreach (var it in SplitStringList)
                        dataSendList.Add(Convert.ToByte(it, 16));
                    if (EtherNetPort != null && EtherNetPort.IsConnection)
                    {
                        {
                            if (EtherNetPort.WriteData(dataSendList.ToArray(), dataSendList.Count())) //将生成的发送出去
                            {
                                Thread.Sleep(50);

                                EtherNetPort.ReadData(byteRecv, 1000);   //检查读取到的数据
                                bRet = CheckRecvData(byteRecv);//结束代码是0就是成功
                            }
                        }
                    }
                    return bRet;
                }
            }
            catch (Exception ex)
            {
                nValue = 0;
                return false;
            }
        }
        public bool ReadInt(string strRegisterName,out int nValue)          //批量读取1个字
        {
            try
            {
                lock (_lock)
                {
                    nValue = 0;
                    byte[] byteRecv = new byte[100];
                    ConvertName2Hex(strRegisterName, out string strRegisterAddress);
                    if (EtherNetPort != null && EtherNetPort.IsConnection)
                    {
                        string strCmd = "01,04,00,00,";
                        string strWordNum = "01,00";
                        string strHeader = StrProtocolHeader;
                        strHeader.Replace("DataLength", "0C,00");
                        string strSend = strHeader + strCmd + strRegisterAddress + strWordNum;
                        string[] SplitStringList = strSend.Split(',');

                        int len = SplitStringList.Length;

                        List<byte> dataSendList = new List<byte>();
                        foreach (var it in SplitStringList)
                            dataSendList.Add(Convert.ToByte(it, 16));
                        EtherNetPort.WriteData(dataSendList.ToArray(), dataSendList.Count);
                        Thread.Sleep(50);  //等待再去读取返回值
                        EtherNetPort.ReadData(byteRecv, 100); //检查读取到的数据

                        if (CheckRecvData(byteRecv))//结束代码是0就是成功
                        {
                            nValue = byteRecv[11] + (byteRecv[12] << 8);
                        }

                    }
                    return true;
                }
            }
            catch(Exception ex)
            {
                nValue = 0;
                return false;
            }
        }
        public bool WriteInt(string strRegisterName,int nValue)
        {
            lock (_lock)
            {
                bool bRet = false;
                string strCmd = "01,14,00,00,";  //写命令

                byte[] byteRecv = new byte[100];
                ConvertName2Hex(strRegisterName, out string strRegisterAddress);  //元件代码以及其实地址
                string strNum = "01,00,";            

                //替换发送数据的长度
                string strHeader = StrProtocolHeader;
                strHeader.Replace("DataLength", "0E,00");  //共发送13个字节

                string strData = string.Format("{0},{1},", nValue & 0xFF, (nValue >> 8) & 0xFF);
                string strSend = strHeader + strCmd + strRegisterAddress + strNum + strData;
                string[] SplitStringList = strSend.Split(',');
                int len = SplitStringList.Length;
                List<byte> dataSendList = new List<byte>();
                foreach (var it in SplitStringList)
                    dataSendList.Add(Convert.ToByte(it, 16));

                if (EtherNetPort != null && EtherNetPort.IsConnection)
                {
                    EtherNetPort.WriteData(dataSendList.ToArray(), dataSendList.Count); //将生成的发送出去
                    Thread.Sleep(50);
                    EtherNetPort.ReadData(byteRecv, 100);   //检查读取到的数据       
                    bRet = CheckRecvData(byteRecv);//结束代码是0就是成功
                }
                else
                    bRet = false;
                return bRet;
            }
        }

        private bool CheckRecvData(byte[] dataArray)
        {
	        return (
                    dataArray.Count()>=11 &&
                    dataArray[0]==0xD0 &&
			        dataArray[1]==0x00 &&
			        dataArray[2]==0x00 &&
			        dataArray[3]==0xFF &&
			        dataArray[4]==0xFF &&
			        dataArray[5]==0x03 &&
			        dataArray[6]==0x00 &&
			        dataArray[9]==0x00 &&
			        dataArray[10]==0x00);	
        }
        private bool ConvertName2Hex(string strNameSrc, out string strDes)
        {
            //D100
            strDes = "";
            if (strNameSrc.Length < 1)
                return false;
            string subStr = "";
            string str = strNameSrc.Substring(0,1);
            if (!Int16.TryParse(strNameSrc.Substring(1, strNameSrc.Length - 1), out short nValue))
                return false;
            switch (str)
            {
                case "M":
                case "m":
                    subStr = "90,";
                    break;
                case "W":
                case "w":
                    subStr = "A8,";
                    break;
                case "D":
                case "d":
                    subStr = "B4,";
                    break;
                case "T":
                case "t":
                    subStr = "C2,";
                    break;
                case "R":
                case "r":
                    subStr = "AF,";
                    break;
                default:
                    strDes = "";
                    return false;
            }
            strDes = string.Format("{0:X},{1:X},{2:X},{3:X},", nValue & 0XFF, (nValue >> 8) & 0xFF, (nValue >> 16) & 0xFF,subStr);
            return true;
        }
    }
}
