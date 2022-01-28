
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
            instance = this;
            var ass = Assembly.GetExecutingAssembly();
            I18N.AddLanguage(Language.LanguageCode.EN, ass.GetManifestResourceStream("HKTool.Languages.en.txt"));
            I18N.AddLanguage(Language.LanguageCode.ZH, ass.GetManifestResourceStream("HKTool.Languages.zh-cn.txt"));

            I18N.UseGameLanguage();

            IsDebugMode = settings.DevMode;
            if (settings.DevMode)
            {
                DebugTools.DebugManager.Init();

                if (settings.DebugConfig.DebugMods?.Count > 0)
                {
                    DebugModsLoader.LoadMods(settings.DebugConfig.DebugMods);
                }
            }
        }

        public static HKToolSettings settings = new HKToolSettings();
        public bool ToggleButtonInsideMenu => true;
        public static HKToolSettingsMenu SettingsMenu;
        public void SaveSettings() => SaveGlobalSettings();
        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            SettingsMenu = new(modListMenu);
            if(IsDebugMode)
            {
                SaveModifyMenu.instance = new(SettingsMenu);
            }
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
