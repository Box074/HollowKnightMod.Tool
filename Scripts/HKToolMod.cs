
namespace HKTool;

class HKToolMod : ModBase<HKToolMod>, IGlobalSettings<HKToolSettings>, ICustomMenuMod
{
    public static List<WeakReference<FsmState>> ignoreLoadActionsState = new();
    static HKToolMod()
    {
        On.HutongGames.PlayMaker.FsmState.LoadActions += (orig, self) =>
        {
            if (ignoreLoadActionsState.Any(x => x.TryGetTarget(out var state) && ReferenceEquals(state, self)))
            {
                return;
            }
            orig(self);
        };

        if (CurrentMAPIVersion < CompileInfo.SUPPORT_PRELOAD_ASSETS_VERSION && CurrentMAPIVersion >= CompileInfo.SUPPORT_PRELOAD_PREFAB_VERSION)
        {
            try
            {
                void NewAPIPatch()
                {
                    HookEndpointManager.Modify(FindType("Modding.Preloader+<>c__DisplayClass22_0")!.GetMethods(HReflectionHelper.All).First(
                        x => x.Name.StartsWith("<DoPreload>g__GetPreloadObjectsOperation")),
                        (ILContext context) =>
                        {
                            var cursor = new ILCursor(context);
                            cursor.GotoNext(MoveType.Before, x => 
                            {
                                if(x.OpCode != MOpCodes.Callvirt) return false;
                                var mr = (MethodReference)x.Operand;
                                if(mr.DeclaringType.GetElementType().FullName != typeof(Dictionary<,>).FullName) return false;
                                if(mr.Name != "Add") return false;
                                return true;
                            });
                            cursor.Next.Operand = typeof(Dictionary<string, GameObject>).GetMethod("set_Item");
                        });
                    HookEndpointManager.Modify(FindMethodBase("Modding.Preloader+<DoPreload>d__22::MoveNext"),
                        (ILContext context) =>
                        {
                            var cursor = new ILCursor(context);
                            cursor.GotoNext(MoveType.Before, x => 
                            {
                                if(x.OpCode != MOpCodes.Callvirt) return false;
                                var mr = (MethodReference)x.Operand;
                                if(mr.DeclaringType.GetElementType().FullName != typeof(Dictionary<,>).FullName) return false;
                                if(mr.Name != "Add") return false;
                                return true;
                            });
                            cursor.Next.Operand = typeof(Dictionary<string, GameObject>).GetMethod("set_Item");
                        });
                }
                NewAPIPatch();
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
        }

    }
    public static I18n I18N => Instance.I18n;
    public static SimpleLogger unityLogger = new("UNITY");
    public static SimpleLogger logger = new("HKTool");
    public static bool IsDebugMode { get; private set; }
    public static ReflectionObject RModLoader => ModLoaderHelper.RModLoader;
    public HKToolMod() : base("HKTool")
    {

        IsDebugMode = settings.DevMode;

        try
        {
            Init();
        }
        catch (Exception e)
        {
            LogError(e);
        }

        I18n.AddLanguage(SupportedLanguages.EN, ModRes.LANGUAGE_EN);
        I18n.AddLanguage(SupportedLanguages.ZH, ModRes.LANGUAGE_ZH);
        I18n.UseGameLanguage(SupportedLanguages.EN, true);

        On.HeroController.get_instance += (_) => HeroController.SilentInstance;
        if (settings.DevMode)
        {
            DebugTools.DebugManager.Init();
            DebugManager.Init();
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
    private static string GetUnityLogStackTrace(LogType logType, string stackTrace = "")
    {
        /*if (devSettings.UnityLogStackTraceType is null ||
            devSettings.UnityLogStackTraceType.Length <= (int)logType)
        {
            return stackTrace;
        }
        var stt = devSettings.UnityLogStackTraceType[(int)logType];
        if(stt == StackTraceLogType.None) return "";
        if(stt == StackTraceLogType.Full) return stackTrace;
        StackTrace st = new(3, false);
        return st.ToString();*/
        return stackTrace;
    }
    private static void UnityLogHandler(string msg, string stackTrace, LogType logType)
    {
        if (logType == LogType.Error && settings.DebugConfig.rUnityError)
        {
            unityLogger.LogError($"{msg}\n{GetUnityLogStackTrace(logType, stackTrace)}");
        }
        else if (logType == LogType.Warning && settings.DebugConfig.rUnityWarn)
        {
            unityLogger.LogWarn($"{msg}\n{GetUnityLogStackTrace(logType, stackTrace)}");
        }
        else if (logType == LogType.Log && settings.DebugConfig.rUnityLog)
        {
            unityLogger.Log($"{msg}\n{GetUnityLogStackTrace(logType, stackTrace)}");
        }
        else if (logType == LogType.Exception && settings.DebugConfig.rUnityException)
        {
            unityLogger.LogError($"[EXCEPTION]{msg}\n{GetUnityLogStackTrace(logType, stackTrace)}");
        }
        else if (logType == LogType.Assert && settings.DebugConfig.rUnityAssert)
        {
            unityLogger.LogError($"[ASSERT]{msg}\n{GetUnityLogStackTrace(logType, stackTrace)}");
        }
    }
    public static bool i18nShowOrig;

    internal static void Init()
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

    public override string MenuButtonName => "HKTool.Menu.ButtonLabel".Localize();
    public override Font MenuButtonLabelFont => MenuResources.Perpetua;
    public static HKToolSettings settings = HKToolSettings.TryLoad();
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
    public void OnLoadGlobal(HKToolSettings s) {}
    public HKToolSettings OnSaveGlobal() => settings;
}

