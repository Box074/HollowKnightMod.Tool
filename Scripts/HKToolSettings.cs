

namespace HKTool;
[Serializable]
class HKToolSettings
{
    public static bool TestMode = false;
    public bool DevMode = false;
    public HKToolDebugConfig DebugConfig { get; set; } = new();
    public HKToolExperimentalConfig ExperimentalConfig { get; set; } = new();
}
[Serializable]
class HKToolExperimentalConfig
{
    public bool satchel_use_shader_extract = false;
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

