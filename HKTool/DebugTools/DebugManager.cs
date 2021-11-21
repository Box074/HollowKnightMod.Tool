using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.DebugTools
{
    class DebugManager
    {
        public static void Init()
        {
            DebugView.Init();
            DebugView.debugViews.Add(new HKDebug.Menu.MenuShow());
            HKDebug.UnityExplorer.Init();
            HKDebug.Tool.Init();
            HKDebug.FakeDebug.Init();
            HKDebug.HitBox.HitBoxCore.Init();
        }
    }
}
