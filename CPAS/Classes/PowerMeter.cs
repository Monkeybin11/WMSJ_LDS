using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
namespace CPAS.Classes
{
    public class PowerMeter
    {
        private byte[] byteRecv = new byte[64];
        private SerialPort sp = null;
        private PowerMeter() { }
        public PowerMeter(SerialPort sp)
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
       
    }
}
