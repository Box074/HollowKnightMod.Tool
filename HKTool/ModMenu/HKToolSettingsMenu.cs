
namespace HKTool;

class HKToolSettingsMenu : CustomMenu
{
    public static bool init = false;
    public override Font titleFont => MenuResources.Perpetua;
    public static HKToolSettings settings => HKToolMod2.settings;
    public override bool DelayBuild => true;
    private DebugModuleMenu debugModule;
    public HKToolSettingsMenu(MenuScreen returnScreen) : base(returnScreen, "HKTool")
    {
        LogMenu.instance = new(this);
        TestMenu.instance = new(this);
        debugModule = new(this);
    }
    protected override void Back()
    {
        base.Back();
        HKToolMod2.Instance.SaveSettings();
    }
    private void DebugOptions()
    {

        AddButton("HKTool.Settings.DebugView".Localize(), "HKTool.Settings.DebugView.Desc".Localize(),
            () =>
            {
                DebugView.IsEnable = !DebugView.IsEnable;
            }, MenuResources.Perpetua);

        AddButton("Debug Modules", "",
            () =>
            {
                debugModule.Refresh();
                GoToMenu(debugModule);
            }, MenuResources.Perpetua);


        AddButton("HKTool.LogMenu.Title".Localize(), "",
        () =>
        {
            LogMenu.instance!.Refresh();
            GoToMenu(LogMenu.instance);
        }, MenuResources.Perpetua);
        AddBoolOption("HKTool.I18n.ShowOrigin".Localize(),
            "HKTool.Desc.NeverSave".Localize(),
            new FieldRefHolder<bool>(HKToolMod2.Instance, nameof(HKToolMod2.i18nShowOrig)), null, MenuResources.Perpetua);
        AddButton("HKTool.Menu.RebuildMenu".Localize(), "",
            () =>
            {
                foreach (var v in menus)
                {
                    try
                    {
                        v.Rebuild();
                    }
                    catch (Exception e)
                    {
                        HKToolMod2.logger.LogError(e);
                    }
                    Back();
                }
            }, MenuResources.Perpetua);
        AddButton("Test Options", "",
        () =>
        {
            if (TestMenu.instance is not null) GoToMenu(TestMenu.instance);
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
                HKToolMod2.Instance.SaveSettings();
            }, () =>
            {
                return settings.DevMode ? 1 : 0;
            }, MenuResources.Perpetua);

        AddButton("HKTool.Experimental.Title".Localize(), "",
        () =>
        {
            if (ExperimentalMenu.instance is null)
            {
                ExperimentalMenu.instance = new(this);
            }
            ExperimentalMenu.instance.Refresh();
            GoToMenu(ExperimentalMenu.instance);
        }, MenuResources.Perpetua);
        AddButton("HKTool.Menu.RefreshLanguage".Localize(), "",
            () =>
            {
                foreach (var v in I18n.Instances)
                {
                    v.TrySwitch();
                }
                foreach (var v in menus)
                {
                    try
                    {
                        v.Rebuild();
                    }
                    catch (Exception e)
                    {
                        HKToolMod2.logger.LogError(e);
                    }
                    Back();
                }
            }, MenuResources.Perpetua);

        if (HKToolMod2.IsDebugMode)
        {
            DebugOptions();
        }
    }
}

