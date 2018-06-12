using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPAS.Config
{
    public class ConfigMgr
    {
        private ConfigMgr() { }
        private static readonly Lazy<ConfigMgr> _instance = new Lazy<ConfigMgr>(() => new ConfigMgr());
        public static ConfigMgr Instance
        {
            get { return _instance.Value; }
        }
        public void LoadConfig()
        { 
            
        }
    }
}
