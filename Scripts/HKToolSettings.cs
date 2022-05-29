

namespace HKTool;
[Serializable]
class HKToolSettings
{
    public static bool TestMode = false;
    public bool DevMode = false;
    public HKToolDebugConfig DebugConfig { get; set; } = new HKToolDebugConfig();
    public bool EmulateNewMAPIFeatures = true;
}
[Serializable]
class HKToolDebugConfig
{
    public List<string> DebugMods { get; set; } = new List<string>();
    public StackTraceLogType[]? UnityLogStackTraceType { get; set; } = null;
    public bool rUnityLog;
    public bool rUnityWarn;
    public bool rUnityError;
    public bool rUnityException;
    public bool rUnityAssert;
}

