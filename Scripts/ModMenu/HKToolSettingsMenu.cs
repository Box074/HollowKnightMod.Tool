
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
        LogMenu.instance = new(this);
        AddButton("HKTool.Settings.DebugView".Get(), "HKTool.Settings.DebugView.Desc".Get(),
            () =>
            {
                DebugTools.DebugView.IsEnable = !DebugTools.DebugView.IsEnable;
            }, MenuResources.Perpetua);

        AddButton("HKTool.Menu.ModifySaveTitle".Get(), "",
        () =>
        {
            if (SaveModifyMenu.instance != null)
            {
                GoToMenu(SaveModifyMenu.instance);
            }
        }, MenuResources.Perpetua);
        AddButton("HKTool.LogMenu.Title".Get(), "",
        () => 
        {
            LogMenu.instance.Refresh();
            GoToMenu(LogMenu.instance);
        }, MenuResources.Perpetua);
        AddBoolOption("HKTool.I18n.ShowOrigin".Get(),
            "HKTool.Desc.NeverSave".Get(),
            (val) => {
                HKToolMod.i18nShowOrig = val;
            },
            () => HKToolMod.i18nShowOrig, MenuResources.Perpetua);
    }
    protected override void Build(ContentArea contentArea)
    {
        autoRefresh = true;

        AddOption("HKTool.Settings.DeveloperMode".Get(), "HKTool.Settings.DeveloperMode.Desc".Get(),
            new string[] { "HKTool.Settings.Off".Get(), "HKTool.Settings.On".Get() },
            (id) =>
            {
                settings.DevMode = id == 1;
                HKToolMod.Instance.SaveSettings();
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

