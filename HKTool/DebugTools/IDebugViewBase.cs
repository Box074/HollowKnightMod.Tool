

namespace HKTool.DebugTools;
public interface IDebugViewBase
{
    void OnDebugDraw();
    string GetViewName();
    bool FullScreen { get; }
}

