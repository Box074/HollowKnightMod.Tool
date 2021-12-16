using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.FSM
{
    public interface IFsmFilter
    {
        bool Filter(PlayMakerFSM pm);
    }
}
