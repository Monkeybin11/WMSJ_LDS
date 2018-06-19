using CPAS.Config;
using CPAS.Config.HardwareManager;
using Keyence.AutoID.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Instrument 
{
    public class Keyence_SR1000 : InstrumentBase
    {
        private byte[] byteRecv = new byte[1024 * 2];
        private ComportCfg comportCfg = null;
        private EtherNetCfg etherNetCfg=null;  
        public Keyence_SR1000(HardwareCfgLevelManager1 cfg) : base(cfg)
        {

        }
        private ReaderAccessor BarcodeReader = null;
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
                else if (Config.ConnectMode.ToUpper() == @"ETHERNET")
                {
                    foreach (var it in hardwareCfg.EtherNets)
                    {
                        if (it.PortName == Config.PortName)
                            etherNetCfg = it;
                    }
                    if (etherNetCfg == null)
                        return false;
                      ReaderAccessor BarcodeReader = new ReaderAccessor();
                      BarcodeReader.IpAddress = etherNetCfg.IP;
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
        public bool Connect()
        {
            return BarcodeReader.Connect();
        }
        public string Getbarcode()
        {
            return BarcodeReader.ExecCommand("LON");
        }

    }
}
