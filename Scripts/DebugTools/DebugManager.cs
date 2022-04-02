
namespace HKTool.DebugTools;
class DebugManager
{
    public static void Init()
    {
        DebugView.Init();
        AddDebugView(new HKDebug.Menu.MenuShow());
        HKDebug.Tool.Init();
        HKDebug.FakeDebug.Init();
    }

    public static void AddDebugView(IDebugViewBase view)
    {
        DebugView.debugViews.Add(view);
    }
}

