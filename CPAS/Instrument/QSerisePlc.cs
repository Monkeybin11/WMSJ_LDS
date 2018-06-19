using CPAS.Config;
using CPAS.Config.HardwareManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Instrument
{
    public class QSerisePlc : InstrumentBase
    {

        private byte[] byteRecv = new byte[1024 * 2];
        private ComportCfg comportCfg = null;
        private EtherNetCfg etherNetCfg = null;
        public QSerisePlc(HardwareCfgLevelManager1 cfg) : base(cfg) { }

        public override bool DeInit()
        {
            throw new NotImplementedException();
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
                                etherNetCfg = it;
                        }
                        if (etherNetCfg == null)
                            return false;
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
        public Int32 ReadDint(string strRegisterName)
        {
            return 0;
        }
        public bool WriteDint(string strRegisterName)
        {
            return true;
        }
    }
}
