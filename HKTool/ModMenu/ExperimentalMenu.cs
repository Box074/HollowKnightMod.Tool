
namespace HKTool.ModMenu;

class ExperimentalMenu : CustomMenu
{
    public static ExperimentalMenu instance = null!;
    private bool cacheAIBM = HKToolMod2.settings.ExperimentalConfig.allow_init_before_mapi;
    public ExperimentalMenu(MenuScreen rs) : base(rs, "HKTool.Experimental.Title".Localize())
    {
        OnChangeAIBM();
    }
    private void OnChangeAIBM()
    {
        if(HKToolMod2.settings.ExperimentalConfig.allow_init_before_mapi)
        {
            InitManager.InstallInit();
        }
        else
        {
            InitManager.UninstallInit();
        }
        if(HKToolMod2.settings.ExperimentalConfig.allow_init_before_mapi != cacheAIBM)
        {
            cacheAIBM = HKToolMod2.settings.ExperimentalConfig.allow_init_before_mapi;
            Rebuild();
            GoToMenu(this);
        }
    }
    protected override void Build(ContentArea contentArea)
    {
        AddBoolOption("HKTool.Experimental.AIBM.Title".Localize(), "", new(HKToolMod2.settings.ExperimentalConfig, "allow_init_before_mapi"), 
            OnChangeAIBM, FontPerpetua);
        if(HKToolMod2.settings.ExperimentalConfig.allow_init_before_mapi)
        {
            AddBoolOption("HKTool.Experimental.StartWithoutSteam".Localize(), "", new FieldRefHolder<bool>(HKToolMod2.settings.ExperimentalConfig, "allow_start_without_steam"), null, FontPerpetua);
        }
    }
}
