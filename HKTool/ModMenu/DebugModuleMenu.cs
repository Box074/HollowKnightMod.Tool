
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
                    HKToolMod2.logger.Log($"Enable Debug Module: {v.DisplayName}");
                    HKToolMod2.devSettings.disabledModules.Remove(v.ModuleName);
                    if(v.CanRuntimeEnabled) v.OnEnable();
                }
                else
                {
                    HKToolMod2.logger.Log($"Disable Debug Module: {v.DisplayName}");
                    HKToolMod2.devSettings.disabledModules.Add(v.ModuleName);
                    if(v.CanRuntimeDisabled) v.OnDisable();
                }
            }, () => !HKToolMod2.devSettings.disabledModules.Contains(v.ModuleName), FontPerpetua);
        }
    }
}
