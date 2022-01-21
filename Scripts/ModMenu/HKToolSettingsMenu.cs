
namespace HKTool;

class HKToolSettingsMenu : CustomMenu
{
    public static HKToolSettings settings => HKToolMod.settings;
    public HKToolSettingsMenu(MenuScreen returnScreen) : base(returnScreen, "HKTool")
    {

    }
    protected override void Build(ContentArea contentArea)
    {
        AddOption("HKTool.Settings.DeveloperMode".Get(), "HKTool.Settings.DeveloperMode.Desc".Get(),
            new string[] { "HKTool.Settings.Off".Get(), "HKTool.Settings.On".Get() },
            (id) =>
            {
                settings.DevMode = id == 1;
                HKToolMod.instance.SaveSettings();
            }, ()=>{
                return settings.DevMode ? 1 : 0;
            });
        if(HKToolMod.IsDebugMode)
        {
            AddButton("HKTool.Settings.DebugView".Get(), "HKTool.Settings.DebugView.Desc".Get(), 
            () =>
            {
                DebugTools.DebugView.IsEnable = !DebugTools.DebugView.IsEnable;
            });
            AddButton("HKTool.Menu.ModifySaveTitle".Get(), "",
            () => {
                if(SaveModifyMenu.instance != null)
                {
                    GoToMenu(SaveModifyMenu.instance);
                }
            });
        }
    }
}

