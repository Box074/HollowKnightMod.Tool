
namespace HKTool.ModMenu;

class ExperimentalMenu : CustomMenu
{
    public static ExperimentalMenu instance = null!;
    private bool cacheAIBM = HKToolMod.settings.ExperimentalConfig.allow_init_before_mapi;
    public ExperimentalMenu(MenuScreen rs) : base(rs, "HKTool.Experimental.Title".Localize())
    {
        OnChangeAIBM();
    }
    private void OnChangeAIBM()
    {
        if(HKToolMod.settings.ExperimentalConfig.allow_init_before_mapi)
        {
            InitManager.InstallInit();
        }
        else
        {
            InitManager.UninstallInit();
        }
        if(HKToolMod.settings.ExperimentalConfig.allow_init_before_mapi != cacheAIBM)
        {
            cacheAIBM = HKToolMod.settings.ExperimentalConfig.allow_init_before_mapi;
            Rebuild();
            GoToMenu(this);
        }
    }
    protected override void Build(ContentArea contentArea)
    {
        AddBoolOption("HKTool.Experimental.AIBM.Title".Localize(), "", ref HKToolMod.settings.ExperimentalConfig.allow_init_before_mapi, 
            OnChangeAIBM, FontPerpetua);
        if(HKToolMod.settings.ExperimentalConfig.allow_init_before_mapi)
        {
            AddBoolOption("HKTool.Experimental.StartWithoutSteam".Localize(), "", ref HKToolMod.settings.ExperimentalConfig.allow_start_without_steam, null, FontPerpetua);
        }
    }
}
