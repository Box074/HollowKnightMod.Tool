
namespace HKTool.Debugger;

internal abstract class DebugModule
{
    public abstract void OnEnable();
    public virtual void OnDisable() { }
    public virtual bool CanRuntimeDisabled => false;
    public virtual bool CanRuntimeEnabled => false;
    public virtual string ModuleName => GetType().Name;
    public virtual string DisplayName => ModuleName;
}
