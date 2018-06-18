using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Config.HardwareManager
{
    public class HardwareCfgManager
    {
        public PowerMeteConfig[] PowerMetes { get; set; }
        public LDSConfig[] LDSs { get; set; }
        public ComportCfg[] Comports { get; set; }
    }
}
