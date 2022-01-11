using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HKTool.DebugTools
{
    public interface IDebugViewBase
    {
        void OnDebugDraw();
        string GetViewName();
        bool FullScreen { get; }
    }
}
