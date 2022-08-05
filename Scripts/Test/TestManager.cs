
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
            Test0().StartCoroutine().Start();
        }),
        ("Test 0", () => {
            TestBeh.Instance.StartCoroutine(Test2());
        })
    };
    public static IEnumerator Test1()
    {
        yield return null;
        throw null!;
    }
    public static IEnumerator Test0()
    {
        yield return Test1();
    }
    public static IEnumerator Test2()
    {
        yield return null;
        yield return Test0();
    }
}
