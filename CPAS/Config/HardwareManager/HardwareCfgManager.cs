using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Config.HardwareManager
{
    public class HardwareCfgManager
    {
        public PowerMeteConfig[] PowerMeters { get; set; }
        public LDSConfig[] LDSs { get; set; }
        public PLCConfig[] QSerisePlcs { get; set; }
        public KeyenceReaderConfig[] Keyence_SR1000s { get; set; }


        public ComportCfg[] Comports { get; set; }
        public EtherNetCfg[] EtherNets { get; set; }
        public NIVasaCfg[] NIVisas { get; set; }
    }
}
