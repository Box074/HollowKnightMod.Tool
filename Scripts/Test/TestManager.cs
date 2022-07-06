
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
            ILTest.Test02();
        }),
        ("ILTest3", () => {
            FsmInt i = 1024;
            HKToolMod.logger.Log($"T {i.private_value()}");
            i.private_value() = 2048;
            HKToolMod.logger.Log($"T2 {i.private_value()}");
            HKToolMod.logger.Log($"T3 {i.Value}");
        })
    };
}
