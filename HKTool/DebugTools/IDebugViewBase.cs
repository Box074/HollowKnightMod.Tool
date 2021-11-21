using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.DebugTools
{
    public interface IDebugViewBase
    {
        void OnDebugDraw();
        string GetModName();
    }
}
