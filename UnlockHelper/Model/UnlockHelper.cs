using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UnlockHelper.Model
{
   
    public class UnlockHelper
    {
        [DllImport("LdsUnlockLibrary.dll")]
        public static extern bool LdsUnlock(ref string[] result);
    }

}
