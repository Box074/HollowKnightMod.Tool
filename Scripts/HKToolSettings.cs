
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
        public List<string> DebugMods { get; set; } = new List<string>();
        public HitBoxConfig HitBoxConfig { get; set; } = null;
        public StackTraceLogType[] UnityLogStackTraceType { get; set; } = null;
        public bool rUnityLog { get; set; }
        public bool rUnityWarn { get; set; }
        public bool rUnityError { get; set; }
        public bool rUnityException { get; set; }
        public bool rUnityAssert { get; set; }
    }
}
