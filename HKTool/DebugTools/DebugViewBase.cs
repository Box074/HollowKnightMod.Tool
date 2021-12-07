using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HKTool.DebugTools
{
    public abstract class DebugViewBase : IDebugViewBase
    {
        public virtual bool FullScreen => false;

        public abstract string GetViewName();
        public abstract void OnDebugDraw();
    }
}
