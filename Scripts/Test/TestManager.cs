
namespace HKTool.Test;

static class TestManager
{
    public static (string name, Action onClick)[] tests = new(string name, Action onClick)[]{
        ("CSFsm", () => {
            var go = new GameObject("CSFsm Test");
            CSFsmTest.Attach(go, "Test0");
        })
    };
}
