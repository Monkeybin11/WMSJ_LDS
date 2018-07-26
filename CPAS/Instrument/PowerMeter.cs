using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using CPAS.Config.HardwareManager;
using CPAS.Config;
using NationalInstruments.VisaNS;
using Thorlabs.TLPM_32.Interop;
using System.Runtime.InteropServices;

namespace CPAS.Instrument
{
    public enum EnumUnit
    {
        W,
        mW,
        μW
    }
    public class PowerMeter : InstrumentBase
    {
        private byte[] byteRecv = new byte[64];
        ComportCfg comportCfg = null;
        private TLPM tlpm;
        public double[] MeasureValue=new double[4] { 0.0f,0.0f,0.0f,0.0f};
        MessageBasedSession session = null;
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
                else if (Config.ConnectMode.ToUpper() == @"NIVISA")
                {
                    HandleRef Instrument_Handle = new HandleRef();
                    TLPM searchDevice = new TLPM(Instrument_Handle.Handle);
                    uint count = 0;
                    int pInvokeResult = searchDevice.findRsrc(out count);
                    if (count == 0)
                    {
                        searchDevice.Dispose();
                        return false;
                    }
                    foreach (var it in hardwareCfg.NIVisas)
                    {
                        for (uint i=0;i< count;i++)
                        {
                            StringBuilder descr = new StringBuilder(1024);
                            searchDevice.getRsrcName(i, descr);
                            if (descr.ToString().Contains(it.KeyWord1))
                            {
                                tlpm = new TLPM(descr.ToString(), false, false);
                                return tlpm != null;
                            }
                        }        
                    }

                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool MyInit(string keywords)
        {
            try
            {

                    HandleRef Instrument_Handle = new HandleRef();
                    TLPM searchDevice = new TLPM(Instrument_Handle.Handle);
                    uint count = 0;
                    int pInvokeResult = searchDevice.findRsrc(out count);
                    if (count == 0)
                    {
                        searchDevice.Dispose();
                        return false;
                    }
                
                    for (uint i = 0; i < count; i++)
                    {
                        StringBuilder descr = new StringBuilder(1024);
                        searchDevice.getRsrcName(i, descr);
                        if (descr.ToString().Contains(keywords))
                        {
                            tlpm = new TLPM(descr.ToString(), false, false);
                            return tlpm != null;
                        }
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
            if (session != null)
            {
                session.Clear();
            }
            return true;
        }
        public  object Excute(object objCmd)
        {
            try
            {
                lock (_lock)
                {
                    session.Write(objCmd.ToString());
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
                lock (_lock)
                {
                    return session.Query(objCmd.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public double GetPowerValue(EnumUnit unit)
        {
            int err = tlpm.measPower(out double value);

            if (err==0)
            {
                int n = 1;
                switch (unit)
                {
                    case EnumUnit.mW:
                        n = 1000;
                        break;
                    case EnumUnit.μW:
                        n = 1000000;
                            break;
                }
                return value*n;
            }
            return 0.0f;
        }
        public void Abort()
        {
            Excute(":ABOR");
        }
    }
}
