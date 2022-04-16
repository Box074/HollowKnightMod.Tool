
namespace HKTool;

class HKToolSettingsMenu : CustomMenu
{
    public static bool init = false;
    public override Font titleFont => MenuResources.Perpetua;
    public static HKToolSettings settings => HKToolMod.settings;
    public MenuButton? modifySaveButton;
    public override bool DelayBuild => true;
    public HKToolSettingsMenu(MenuScreen returnScreen) : base(returnScreen, "HKTool")
    {
        LogMenu.instance = new(this);
        TestMenu.instance = new(this);
    }
    protected override void OnEnterMenu()
    {
        if(modifySaveButton is null) return;
        if(GameManager.instance.gameState != GameState.MAIN_MENU)
        {
            modifySaveButton.SetInteractable(false, "HKTool.Menu.OnlyMainMenu".Get());
        }
        else
        {
            modifySaveButton.SetInteractable(true);
        }
    }
    private void DebugOptions()
    {
        
        AddButton("HKTool.Settings.DebugView".Get(), "HKTool.Settings.DebugView.Desc".Get(),
            () =>
            {
                DebugTools.DebugView.IsEnable = !DebugTools.DebugView.IsEnable;
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
        AddButton("HKTool.Menu.RebuildMenu".Get(), "",
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

        AddOption("HKTool.Settings.DeveloperMode".Get(), "HKTool.Settings.DeveloperMode.Desc".Get(),
            new string[] { "HKTool.Settings.Off".Get(), "HKTool.Settings.On".Get() },
            (id) =>
            {
                settings.DevMode = id == 1;
                HKToolMod.Instance.SaveSettings();
            }, () =>
            {
                return settings.DevMode ? 1 : 0;
            }, MenuResources.Perpetua);
        AddButton("HKTool.Menu.RefreshLanguage".Get(), "",
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

