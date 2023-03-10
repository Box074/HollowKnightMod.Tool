

namespace HKTool;
[Serializable]
class HKToolSettings
{
    private static string _globalSettingsPath = Path.Combine(Application.persistentDataPath, "HKToolMod.GlobalSettings.json");
    public static HKToolSettings TryLoad()
    {
        if(_settings is not null) return _settings;
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
            _settings = obj;
            return obj;
        }
        catch (Exception)
        {
            return new();
        }
    }
    private static HKToolSettings? _settings;
    public static HKToolSettings settings => _settings ?? TryLoad();
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
    public bool rUnityLog = false;
    public bool rUnityWarn = false;
    public bool rUnityError = false ;
    public bool rUnityException = false;
    public bool rUnityAssert = false;
    public List<string> disabledModules { get; set; } = new();
}

