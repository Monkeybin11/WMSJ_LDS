using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Config.HardwareManager
{
    public class CameraCfg
    {
        public string Name { get; set; }        //UserName:IP
        public string NameForVision { get; set; }   //Vision use
        public int LightValue { get; set; }
        public string ConnectType { get; set; }
    }
}
