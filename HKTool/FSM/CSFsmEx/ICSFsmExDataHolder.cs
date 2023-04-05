using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.FSM.CSFsmEx
{
    public interface ICSFsmExDataHolder
    {
        void Read(CSFsmHost inst);
        void Write(CSFsmHost inst);
    }
}
