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
    public class PowerMeter : InstrumentBase
    {
        private byte[] byteRecv = new byte[64];
        ComportCfg comportCfg = null;
        public double[] MeasureValue=new double[4] { 0.0f,0.0f,0.0f,0.0f};
        public PowerMeter(HardwareCfgLevelManager1 cfg) : base(cfg)
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
        public  object Excute(object objCmd)
        {
            try
            {
                lock (comPort)
                {
                    comPort.WriteLine(objCmd.ToString());
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public  object Query(object objCmd)
        {
            try
            {
                lock (comPort)
                {
                    comPort.WriteLine(objCmd.ToString());
                    return comPort.ReadLine().Replace("\r", "").Replace("\n", "");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public double GetPowerValue()
        {

            return 0.0f;
        }
        public bool SetContinuous(bool bEnable)
        {
            string strCmd = string.Format(":INITiate:CONTinuous {0}", bEnable ? "ON" : "OFF");
            return (bool)Excute(strCmd);
        }
        public  void Fetch(object o = null)
        {
            SetContinuous(false);
            Thread.Sleep(50);
            string ret = Query(":READ?").ToString();
            string[] meas_ret = ret.Split(',');
            if (meas_ret.Length == 1)
            {
                Double.TryParse(meas_ret[0], out MeasureValue[0]);
            }
        }
        public void Abort()
        {
            Excute(":ABORt");
        }
    }
}
