
namespace HKTool.ModMenu;

internal class DebugModuleMenu : CustomMenu
{
    public DebugModuleMenu(MenuScreen rs) : base(rs) {}
    public override string title => "Debug Module Options";
    protected override void Build(ContentArea contentArea)
    {
        foreach(var v in DebugManager.modules)
        {
            AddBoolOption(v.DisplayName, "", val =>
            {
                if(val)
                {
                    HKToolMod.logger.Log($"Enable Debug Module: {v.DisplayName}");
                    HKToolMod.devSettings.disabledModules.Remove(v.ModuleName);
                    if(v.CanRuntimeEnabled) v.OnEnable();
                }
                else
                {
                    HKToolMod.logger.Log($"Disable Debug Module: {v.DisplayName}");
                    HKToolMod.devSettings.disabledModules.Add(v.ModuleName);
                    if(v.CanRuntimeDisabled) v.OnDisable();
                }
            }, () => !HKToolMod.devSettings.disabledModules.Contains(v.ModuleName), FontPerpetua);
        }
    }
}
