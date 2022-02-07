
namespace HKTool;

class HKToolSettingsMenu : CustomMenu
{
    public override Font titleFont => MenuResources.Perpetua;
    public static HKToolSettings settings => HKToolMod.settings;
    public HKToolSettingsMenu(MenuScreen returnScreen) : base(returnScreen, "HKTool")
    {

    }
    private void DebugOptions()
    {
        SaveModifyMenu.instance = new(this);
        AddButton("HKTool.Settings.DebugView".Get(), "HKTool.Settings.DebugView.Desc".Get(),
            () =>
            {
                DebugTools.DebugView.IsEnable = !DebugTools.DebugView.IsEnable;
            });

        AddButton("HKTool.Menu.ModifySaveTitle".Get(), "",
        () =>
        {
            if (SaveModifyMenu.instance != null)
            {
                GoToMenu(SaveModifyMenu.instance);
            }
        });
        
        for(int i = 0; i < 5 ; i++)
        {
            int id = i;
            AddOption(string.Format("HKTool.Menu.ULST".Get(), Enum.GetName(typeof(LogType), (LogType)id)), 
                "", Enum.GetNames(typeof(StackTraceLogType)),
            (int val) =>
            {
                Application.SetStackTraceLogType((LogType)id, (StackTraceLogType)val);
                HKToolMod.devSettings.UnityLogStackTraceType[id] = (StackTraceLogType) val;
            },
            () => {
                return (int)Application.GetStackTraceLogType((LogType)id);
            });
        }
    }
    protected override void Build(ContentArea contentArea)
    {
        autoRefresh = true;

        AddOption("HKTool.Settings.DeveloperMode".Get(), "HKTool.Settings.DeveloperMode.Desc".Get(),
            new string[] { "HKTool.Settings.Off".Get(), "HKTool.Settings.On".Get() },
            (id) =>
            {
                settings.DevMode = id == 1;
                HKToolMod.instance.SaveSettings();
            }, () =>
            {
                return settings.DevMode ? 1 : 0;
            });
        if (HKToolMod.IsDebugMode)
        {
            DebugOptions();
        }
    }
}

