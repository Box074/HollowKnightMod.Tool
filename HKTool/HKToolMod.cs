using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Modding;

namespace HKTool
{
    class HKToolMod : Mod , IGlobalSettings<HKToolSettings> , IMenuMod
    {
        public static I18n I18N { get; } = new I18n();
        public HKToolMod() : base("HKTool")
        {
            var ass = Assembly.GetExecutingAssembly();
            I18N.AddLang(Language.LanguageCode.EN, ass.GetManifestResourceStream("HKTool.Lang.en.txt"));
            I18N.AddLang(Language.LanguageCode.ZH, ass.GetManifestResourceStream("HKTool.Lang.zh-cn.txt"));

            I18N.ChangeToDefault();

            FSM.FsmManager.Init();

            if (settings.DevMode)
            {
                DebugTools.DebugManager.Init();

                if(settings.DebugConfig.ExternMods?.Count > 0)
                {
                    ExternModsLoader.LoadMods(settings.DebugConfig.ExternMods);
                }
            }
        }

        public static HKToolSettings settings = new HKToolSettings();
        public bool ToggleButtonInsideMenu => true;

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            var list = new List<IMenuMod.MenuEntry>();
            list.Add(new IMenuMod.MenuEntry()
            {
                Name = "HKTool.Settings.DeveloperMode".Get(),
                Values = new string[] { "HKTool.Settings.Off".Get() , "HKTool.Settings.On".Get() },
                Description = "HKTool.Settings.DeveloperMode.Desc".Get(),
                Loader = () => settings.DevMode ? 1 : 0,
                Saver = (val) =>
                {
                    settings.DevMode = val != 0;
                    SaveGlobalSettings();
                }
            });
            if (settings.DevMode)
            {
                list.Add(new IMenuMod.MenuEntry()
                {
                    Name = "HKTool.Settings.LoadSettings".Get(),
                    Saver = (val) =>
                    {
                        GetType().GetMethod("LoadGlobalSettings", BindingFlags.NonPublic | BindingFlags.Instance)
                         .Invoke(this, Array.Empty<object>());
                    },
                    Loader = () => 0,
                    Values = new string[] { "" }
                });
            }
            return list;
        }

        public override string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString() + (settings.DevMode ? "DevMode" : "");
        }

        public void OnLoadGlobal(HKToolSettings s) => settings = s;
        public HKToolSettings OnSaveGlobal() => settings;
    }
}
