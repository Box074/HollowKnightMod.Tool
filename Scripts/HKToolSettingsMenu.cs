
namespace HKTool;

class HKToolSettingsMenu : CustomMenu
{
    public static HKToolSettings settings => HKToolMod.settings;
    public HKToolSettingsMenu(MenuScreen returnScreen) : base(returnScreen, "HKTool")
    {

    }
    protected override void Build(ContentArea contentArea)
    {
        contentArea.AddHorizontalOption("EnableDevMode", new HorizontalOptionConfig()
        {
            Label = "HKTool.Settings.DeveloperMode".Get(),
            Description = new DescriptionInfo()
            {
                Text = "HKTool.Settings.DeveloperMode.Desc".Get()
            },
            RefreshSetting = (self, _) => self.optionList.SetOptionTo(settings.DevMode ? 1 : 0),
            ApplySetting = (self, id) =>
            {
                settings.DevMode = id == 1;
                HKToolMod.instance.SaveSettings();
            },
            Options = new string[] { "HKTool.Settings.Off".Get(), "HKTool.Settings.On".Get() },
            CancelAction = Back,
            Style = HorizontalOptionStyle.VanillaStyle
        }, out var devModeOpt);
        devModeOpt.menuSetting.RefreshValueFromGameSettings();
        if(HKToolMod.IsDebugMode)
        {
            contentArea.AddMenuButton("SHDebugView", new MenuButtonConfig(){
                SubmitAction = (self) => {
                    DebugTools.DebugView.IsEnable = !DebugTools.DebugView.IsEnable;
                },
                Description = new DescriptionInfo(){
                    Text = "HKTool.Settings.DebugView.Desc".Get()
                },
                Label = "HKTool.Settings.DebugView".Get(),
                CancelAction = Back
            });
        }
    }
}

