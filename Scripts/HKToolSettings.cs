

namespace HKTool;
[Serializable]
class HKToolSettings
{
    private static string _globalSettingsPath = Path.Combine(Application.persistentDataPath, "HKToolMod.GlobalSettings.json");
    public static HKToolSettings TryLoad()
    {
        try
        {
            if (!File.Exists(_globalSettingsPath))
                return new();

            using FileStream fileStream = File.OpenRead(_globalSettingsPath);
            using var reader = new StreamReader(fileStream);
            string json = reader.ReadToEnd();

            var obj = JsonConvert.DeserializeObject<HKToolSettings>(
                json,
                new JsonSerializerSettings
                {
                    ContractResolver = ShouldSerializeContractResolver.Instance,
                    TypeNameHandling = TypeNameHandling.Auto,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    Converters = JsonConverterTypes.ConverterTypes
                }
            );
            if (obj == null)
            {
                string globalSettingsBackup = _globalSettingsPath + ".bak";
                if (!File.Exists(globalSettingsBackup))
                    return new();
                fileStream.Close();

                File.Delete(_globalSettingsPath);
                File.Copy(globalSettingsBackup, _globalSettingsPath);
                return TryLoad();
            }

            return obj;
        }
        catch (Exception)
        {
            return new();
        }
    }

    public static bool TestMode = false;
    public bool DevMode = false;
    public HKToolDebugConfig DebugConfig { get; set; } = new();
    public HKToolExperimentalConfig ExperimentalConfig { get; set; } = new();
}
[Serializable]
class HKToolExperimentalConfig
{
    public bool allow_init_before_mapi = false;
    public bool allow_start_without_steam = false;
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

