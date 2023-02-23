
namespace HKTool.Test;

static class TestManager
{
    public static string Test = " A";

    public static IntPtr Pointer;
    public static (string name, Action onClick)[] tests = new(string name, Action onClick)[]{
        ("Access Private Field", () => {
            
        })
    };
}
