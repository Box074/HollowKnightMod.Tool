using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modding;
using HKTool.DebugTools;

namespace HKTool
{
    public static class DebugManager
    {
        public static bool IsDebugMode => HKToolMod.IsDebugMode;
        public static bool IsDebug(Mod mod) => DebugModsLoader.DebugMods.Contains(mod);
        public static Mod[] DebugMods => DebugModsLoader.DebugMods.ToArray();
        public static void AddDebugView(IDebugViewBase view) => DebugTools.DebugManager.AddDebugView(view);
    }
}
