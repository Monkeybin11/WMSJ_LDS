using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Interface
{
    interface Iauthor
    {
        int Level { set; get; }
        void SetLever(int nLever);
        int GetLever();
    }
}
