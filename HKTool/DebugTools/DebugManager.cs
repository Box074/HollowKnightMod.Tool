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
            AddDebugView(new HKDebug.Menu.MenuShow());
            HKDebug.Tool.Init();
            HKDebug.FakeDebug.Init();
            HKDebug.HitBox.HitBoxCore.Init();
        }

        public static void AddDebugView(IDebugViewBase view)
        {
            DebugView.debugViews.Add(view);
        }
    }
}
