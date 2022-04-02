
namespace HKTool;
[ModAllowEarlyInitialization]
class HKToolMod : ModBase<HKToolMod>, IGlobalSettings<HKToolSettings>, ICustomMenuMod
{
    private static int? apiVersion = null;
    public static int ModdingAPIVersion
    {
        get
        {
            if (apiVersion == null)
            {
                apiVersion = int.Parse(ModHooks.ModVersion.Split('-')[1]);
            }
            return apiVersion ?? 0;
        }
    }
    public static I18n I18N => Instance.I18n;
    public static SimpleLogger unityLogger = new("UNITY");
    public static SimpleLogger logger = new("HKTool");
    public static bool IsDebugMode { get; private set; }
    public static ReflectionObject RModLoader => ModLoaderHelper.RModLoader;
    protected override List<(SupportedLanguages, string)> LanguagesEx => new()
    {
        (SupportedLanguages.EN, "HKTool.Languages.en.txt"),
        (SupportedLanguages.ZH, "HKTool.Languages.zh-cn.txt")
    };
    public HKToolMod() : base("HKTool")
    {
        ModListMenuHelper.Init();

        IsDebugMode = settings.DevMode;
        try
        {
            Init();
        }
        catch (Exception e)
        {
            LogError(e);
        }
        //var ass = Assembly.GetExecutingAssembly();
        //I18N.AddLanguage(Language.LanguageCode.EN, EmbeddedResHelper.GetStream(ass, "HKTool.Languages.en.txt"));
        //I18N.AddLanguage(Language.LanguageCode.ZH, EmbeddedResHelper.GetStream(ass, "HKTool.Languages.zh-cn.txt"));

        //I18N.UseGameLanguage();

        HookEndpointManager.Add(typeof(HeroController).GetMethod("get_instance"),
            (Func<HeroController> _) =>
            {
                return HeroController.SilentInstance;
            });


        if (settings.DevMode)
        {
            DebugTools.DebugManager.Init();

            if (settings.DebugConfig.DebugMods?.Count > 0)
            {
                DebugModsLoader.LoadMods(settings.DebugConfig.DebugMods);
            }
        }
    }

    private static void UnityLogStackTrace()
    {
        var ut = devSettings.UnityLogStackTraceType;
        void SetULST()
        {
            ut = new StackTraceLogType[5];
            for (int i = 0; i < 5; i++)
            {
                ut[i] = Application.GetStackTraceLogType((LogType)i);
            }
            devSettings.UnityLogStackTraceType = ut;
        }
        if (ut == null)
        {
            SetULST();
            return;
        }
        if (ut.Length != 5)
        {
            SetULST();
            return;
        }
        for (int i = 0; i < 5; i++)
        {
            Application.SetStackTraceLogType((LogType)i, ut[i]);
        }
    }
    private static void UnityLogHandler(string msg, string stackTrace, LogType logType)
    {
        if (logType == LogType.Error && settings.DebugConfig.rUnityError)
        {
            unityLogger.LogError($"{msg}\n{stackTrace}");
        }
        else if (logType == LogType.Warning && settings.DebugConfig.rUnityWarn)
        {
            unityLogger.LogWarn($"{msg}\n{stackTrace}");
        }
        else if (logType == LogType.Log && settings.DebugConfig.rUnityLog)
        {
            unityLogger.Log($"{msg}\n{stackTrace}");
        }
        else if (logType == LogType.Exception && settings.DebugConfig.rUnityException)
        {
            unityLogger.LogError($"[EXCEPTION]{msg}\n{stackTrace}");
        }
        else if (logType == LogType.Assert && settings.DebugConfig.rUnityAssert)
        {
            unityLogger.LogError($"[ASSERT]{msg}\n{stackTrace}");
        }
    }
    public static bool i18nShowOrig;
    private static void Init()
    {
        if (IsDebugMode)
        {
            Application.logMessageReceived += UnityLogHandler;
            ModHooks.LanguageGetHook += (key, sheet, orig) =>
            {
                if (i18nShowOrig) return $"#{key}({sheet})";
                return orig;
            };
            UnityLogStackTrace();
        }
    }
    
    public override string MenuButtonName => "HKTool.Menu.ButtonLabel".Get();
    public override Font MenuButtonLabelFont => MenuResources.Perpetua;
    public static HKToolSettings settings = new HKToolSettings();
    public static HKToolDebugConfig devSettings => settings.DebugConfig;
    public bool ToggleButtonInsideMenu => true;
    public static HKToolSettingsMenu? SettingsMenu;
    public void SaveSettings() => SaveGlobalSettings();
    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
    {
        SettingsMenu = new(modListMenu);
        return SettingsMenu;
    }
    public override string GetVersion()
    {
        return base.GetVersion() + (settings.DevMode ? "-DevMode" : "");
    }
    public void OnLoadGlobal(HKToolSettings s) => settings = s;
    public HKToolSettings OnSaveGlobal() => settings;
}

