
namespace HKTool.DebugTools;
public abstract class DebugViewBase : IDebugViewBase
{
    public virtual bool FullScreen => false;

    public abstract string GetViewName();
    public abstract void OnDebugDraw();
}

