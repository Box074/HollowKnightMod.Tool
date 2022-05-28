
namespace HKTool;

class HKToolSettingsMenu : CustomMenu
{
    public static bool init = false;
    public override Font titleFont => MenuResources.Perpetua;
    public static HKToolSettings settings => HKToolMod.settings;
    public override bool DelayBuild => true;
    public HKToolSettingsMenu(MenuScreen returnScreen) : base(returnScreen, "HKTool")
    {
        LogMenu.instance = new(this);
        TestMenu.instance = new(this);
    }
    protected override void Back()
    {
        base.Back();
        HKToolMod.Instance.SaveSettings();
    }
    private void DebugOptions()
    {
        
        AddButton("HKTool.Settings.DebugView".Localize(), "HKTool.Settings.DebugView.Desc".Localize(),
            () =>
            {
                DebugTools.DebugView.IsEnable = !DebugTools.DebugView.IsEnable;
            }, MenuResources.Perpetua);

        AddButton("HKTool.LogMenu.Title".Localize(), "",
        () => 
        {
            LogMenu.instance!.Refresh();
            GoToMenu(LogMenu.instance);
        }, MenuResources.Perpetua);
        AddBoolOption("HKTool.I18n.ShowOrigin".Localize(),
            "HKTool.Desc.NeverSave".Localize(),
            (val) => {
                HKToolMod.i18nShowOrig = val;
            },
            () => HKToolMod.i18nShowOrig, MenuResources.Perpetua);
        AddButton("HKTool.Menu.RebuildMenu".Localize(), "",
            () =>
            {
                foreach(var v in CustomMenu.menus)
                {
                    try
                    {
                        v.Rebuild();
                    }
                    catch(Exception e)
                    {
                        HKToolMod.logger.LogError(e);
                    }
                    Back();
                }
            }, MenuResources.Perpetua);
            AddButton("Test Options", "",
            () =>
            {
                if(TestMenu.instance is not null) GoToMenu(TestMenu.instance);
            }, MenuResources.Perpetua);
    }
    protected override void Build(ContentArea contentArea)
    {
        autoRefresh = true;

        AddOption("HKTool.Settings.DeveloperMode".Localize(), "HKTool.Settings.DeveloperMode.Desc".Localize(),
            new string[] { "HKTool.Settings.Off".Localize(), "HKTool.Settings.On".Localize() },
            (id) =>
            {
                settings.DevMode = id == 1;
                HKToolMod.Instance.SaveSettings();
            }, () =>
            {
                return settings.DevMode ? 1 : 0;
            }, MenuResources.Perpetua);
        AddButton("HKTool.Menu.RefreshLanguage".Localize(), "",
            () =>
            {
                foreach(var v in I18n.Instances)
                {
                    v.TrySwitch();
                }
                foreach(var v in CustomMenu.menus)
                {
                    try
                    {
                        v.Rebuild();
                    }
                    catch(Exception e)
                    {
                        HKToolMod.logger.LogError(e);
                    }
                    Back();
                }
            }, MenuResources.Perpetua);
        
        if (HKToolMod.IsDebugMode)
        {
            DebugOptions();
        }
    }
}

