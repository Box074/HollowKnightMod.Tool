
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
        public HKToolRemoteDebugConfig RemoteDebugConfig { get; set; } = new HKToolRemoteDebugConfig();
        public List<string> DebugMods { get; set; } = new List<string>();
        public HitBoxConfig HitBoxConfig { get; set; } = null;
    }
    [Serializable]
    class HKToolRemoteDebugConfig
    {
        public int Port { get; set; } = 8800;
        public bool InternalConsole { get; set; } = false;
    }
}
