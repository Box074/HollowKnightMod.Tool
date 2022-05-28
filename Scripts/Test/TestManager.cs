
namespace HKTool.Test;

static class TestManager
{
    public static string Test = " A";
    public static Ref<string> TestRef = GetRefPointer(ref Test);
    public static IntPtr Pointer;
    public static (string name, Action onClick)[] tests = new(string name, Action onClick)[]{
        ("CSFsm", () => {
            var go = new GameObject("CSFsm Test");
            CSFsmTest.Attach(go, "Test0");
        }),
        ("ILTest", () => {
            string t = "Hello,world!" + Test;
            Pointer = GetRefPointer(ref t);
            ref string r = ref GetFieldRef<string>(null, "HKTool.Test.TestManager::Test");
            HKToolMod.logger.Log($"Hello,World! {r} {r.GetType().FullName}");
            r = "BC";
        }),
        ("ILTest2", () => {
            var r = TestRef.Value;
            HKToolMod.logger.Log($"Hello,World2! {r} {r.GetType().FullName}");
        })
    };
}
