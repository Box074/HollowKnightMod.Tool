
using Modding;
namespace HKTool
{
    class HKToolMod : Mod, IGlobalSettings<HKToolSettings>, ICustomMenuMod
    {
        public static HKToolMod instance { get; private set; }

        public static I18n I18N { get; } = new I18n();
        public static bool IsDebugMode { get; private set; }
        public HKToolMod() : base("HKTool")
        {
            HKToolResourcesAPI.Init();
            IsDebugMode = settings.DevMode;
            instance = this;
            try
            {
                Init();
            }
            catch (Exception e)
            {
                LogError(e);
            }
            var ass = Assembly.GetExecutingAssembly();
            I18N.AddLanguage(Language.LanguageCode.EN, ass.GetManifestResourceStream("HKTool.Languages.en.txt"));
            I18N.AddLanguage(Language.LanguageCode.ZH, ass.GetManifestResourceStream("HKTool.Languages.zh-cn.txt"));

            I18N.UseGameLanguage();

            
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
                for(int i = 0; i < 5; i++)
                {
                    ut[i] = Application.GetStackTraceLogType((LogType)i);
                }
                devSettings.UnityLogStackTraceType = ut;
            }
            if(ut == null)
            {
                SetULST();
                return;
            }
            if(ut.Length != 5)
            {
                SetULST();
                return;
            }
            for(int i = 0; i < 5; i++)
            {
                Application.SetStackTraceLogType((LogType)i, ut[i]);
            }
        }
        private static void Init()
        {
            if(IsDebugMode)
            {
                UnityLogStackTrace();
            }
        }

        public static HKToolSettings settings = new HKToolSettings();
        public static HKToolDebugConfig devSettings => settings.DebugConfig;
        public bool ToggleButtonInsideMenu => true;
        public static HKToolSettingsMenu SettingsMenu;
        public void SaveSettings() => SaveGlobalSettings();
        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            SettingsMenu = new(modListMenu);
            return SettingsMenu;
        }
        public override string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString() + (settings.DevMode ? "-DevMode" : "");
        }
        public void OnLoadGlobal(HKToolSettings s) => settings = s;
        public HKToolSettings OnSaveGlobal() => settings;
    }
}
