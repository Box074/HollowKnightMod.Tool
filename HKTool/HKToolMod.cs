
namespace HKTool;

class HKToolMod2 : ModBase<HKToolMod2>, IGlobalSettings<HKToolSettings>, ICustomMenuMod
{
    public static List<WeakReference<FsmState>> ignoreLoadActionsState = new();
    static HKToolMod2()
    {
        On.HutongGames.PlayMaker.FsmState.LoadActions += (orig, self) =>
        {
            if (ignoreLoadActionsState.Any(x => x.TryGetTarget(out var state) && ReferenceEquals(state, self)))
            {
                return;
            }
            orig(self);
        };

    }
    public static I18n I18N => Instance.I18n;
    public static SimpleLogger unityLogger = new("UNITY");
    public static SimpleLogger logger = new("HKTool");
    public static bool IsDebugMode { get; private set; }
    protected override List<(SupportedLanguages, string)>? LanguagesEx => new()
    {
        (SupportedLanguages.ZH, "HKTool.Resources.Languages.lang_zh.txt"),
        (SupportedLanguages.EN, "HKTool.Resources.Languages.lang_en.txt"),
    };
    public HKToolMod2() : base("HKTool")
    {
        //I18n.AddLanguage(SupportedLanguages.EN, ModResources.LANG_EN);
        //I18n.AddLanguage(SupportedLanguages.ZH, ModResources.LANG_ZH);
        I18n.UseGameLanguage(SupportedLanguages.EN, true);

        if (CurrentMAPIVersion < 72)
        {
            TooOldDependency("Modding API", "72");
        }

        IsDebugMode = settings.DevMode;

        try
        {
            Init();
        }
        catch (Exception e)
        {
            LogError(e);
        }
        On.HeroController.get_instance += HeroController_get_instance;
        //new NativeDetour(FindMethodBase("HeroController::get_instance"), FindMethodBase("HeroController::get_SilentInstance"));
        On.HutongGames.PlayMaker.ReflectionUtils.GetGlobalType += (orig, name) =>
        {
            return orig(name) ?? HReflectionHelper.FindType(name);
        };
        FakePreloadPrefab();
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

    private HeroController HeroController_get_instance(On.HeroController.orig_get_instance orig)
    {
        return HeroController.SilentInstance;
    }

    private void FakePreloadPrefab()
    {
        HashSet<MethodInfo> table = new();
        foreach (var v in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                foreach (var t in v.SafeGetTypes().Where(x => x.IsSubclassOf(typeof(Mod)) && !x.IsAbstract))
                {
                    var getpreloadnames = t.GetMethod("GetPreloadNames", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    if (getpreloadnames is null) continue;
                    if (table.Contains(getpreloadnames)) continue;
                    table.Add(getpreloadnames);
                    HookEndpointManager.Add(getpreloadnames, (Func<Mod, List<(string, string)>> orig, Mod self) =>
                    {
                        var list = orig(self);
                        if (list is null) return list;
                        PreloadObject? modPreload = null;
                        for (int i = 0; i < list.Count; i++)
                        {
                            (string sceneName, string objName) = list[i];
                            if (sceneName.StartsWith("sharedassets", StringComparison.OrdinalIgnoreCase) ||
                                sceneName.StartsWith("resources", StringComparison.OrdinalIgnoreCase))
                            {
                                var name = Path.GetFileNameWithoutExtension(sceneName).ToLower();
                                int? sceneId = null;
                                if (sceneName == "resources")
                                {
                                    sceneId = null;
                                }
                                else
                                {
                                    if (!int.TryParse(name.Substring(12), out var id))
                                    {
                                        continue;
                                    }
                                    sceneId = id;
                                }
                                list.RemoveAt(i);
                                i--;
                                modPreload ??= new();
                                Modding.Logger.LogWarn($"[API Compatibility]'{self.GetName()}' tries to preload '{objName}' using Preload Prefab, which was removed in Modding API 72");
                                this.AddPreloadSharedAsset(sceneId, objName, typeof(GameObject), false, obj =>
                                {
                                    modPreload.TryGetOrAddValue(sceneName, () => new())[objName] = (GameObject)obj!;
                                });

                            }
                            if (modPreload is not null)
                            {
                                ModManager.onLoadMod += (ModInstanceR mi, ref bool updateVer, ref PreloadObject preloads) =>
                                {
                                    if (mi.Mod is null || mi.Mod?.GetType() != t) return;
                                    preloads ??= new();
                                    if (!updateVer)
                                    {
                                        foreach (var v in modPreload)
                                        {
                                            preloads[v.Key] = v.Value!;
                                        }
                                    }
                                };
                            }
                        }
                        return list;
                    });
                }
            }
            catch (Exception)
            {

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
    public static bool i18nShowOrig = false;

    internal static void Init()
    {
        //Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "cecil");
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
        if (AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == "AlreadyEnoughPlayMaker"))
        {
            On.HutongGames.PlayMaker.FsmState.set_Actions += FsmState_set_Actions;
        }
    }

    private static void FsmState_set_Actions(On.HutongGames.PlayMaker.FsmState.orig_set_Actions orig, 
        FsmState self, FsmStateAction[] value)
    {
        orig(self, value);
        if (value != null)
        {
            foreach (var v in value)
            {
                v.Init(self);
            }
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
    public void OnLoadGlobal(HKToolSettings s) { }
    public HKToolSettings OnSaveGlobal() => settings;
}

