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
    public class Keyence_SR1000 : InstrumentBase
    {
        private byte[] byteRecv = new byte[128];
        private ComportCfg comportCfg = null;
        private EtherNetCfg etherNetCfg = null;
        public Keyence_SR1000(HardwareCfgLevelManager1 cfg) : base(cfg)
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
                        if (comPort.IsOpen)
                            comPort.Close();
                        comPort.Open();
                        return comPort.IsOpen;
                    }
                    return false;
                }
                else if (Config.ConnectMode.ToUpper() == @"ETHERNET")
                {
                    foreach (var it in hardwareCfg.EtherNets)
                    {
                        if (it.PortName == Config.PortName)
                            etherNetCfg = it;
                    }
                    if (etherNetCfg == null)
                        return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool MyInit()
        {
            comportCfg = new ComportCfg()
            {
                BaudRate = 115200,
                DataBits = 8,
                Parity = "e",
                Port = "COM8",
                PortName = "SR1_Comport",
                StopBits = 1,
                TimeOut = 1000

            };
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
        public override bool DeInit()
        {
            if (comPort != null)
            {
                comPort.Close();
                comPort.Dispose();
            }
            return true;
        }
        public string Getbarcode()
        {
            string strCode = Query("LON\r").ToString();
            Excute("LOFF\r");
            return strCode;
        }
        public object Excute(object objCmd)
        {
            try
            {
                lock (comPort)
                {
                    comPort.Write(objCmd.ToString());
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public object Query(object objCmd)
        {
            try
            {
                lock (comPort)
                {
                    Array.Clear(byteRecv, 0, byteRecv.Length);
                    comPort.Write(objCmd.ToString());
                    Thread.Sleep(100);
                    comPort.Read(byteRecv, 0, 128);
                    return System.Text.Encoding.ASCII.GetString(byteRecv).Replace("\0", "").Replace("\r", "");
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }

    }
}
