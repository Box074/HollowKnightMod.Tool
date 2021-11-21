using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKDebug.HitBox;

namespace HKTool
{
    [Serializable]
    class HKToolSettings
    {
        public bool DevMode { get; set; } = false;
        public HKToolDebugConfig DebugConfig { get; set; } = new HKToolDebugConfig(); 
    }
    [Serializable]
    class HKToolDebugConfig
    {
        public List<string> ExternMods { get; set; } = new List<string>();
        public HitBoxConfig HitBoxConfig { get; set; } = null;
    }
}
