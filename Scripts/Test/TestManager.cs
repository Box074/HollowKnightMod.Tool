
namespace HKTool.Test;

static class TestManager
{
    public static string Test = " A";
    public static Ref<string> TestRef = GetRefPointer(ref Test);
    public static IntPtr Pointer;
    public static (string name, Action onClick)[] tests = new(string name, Action onClick)[]{
        ("Access Private Field", () => {
            TestAPF();
        })
    };
    [CustomPatcher(typeof(TestManager), nameof(Patch_TestAPF))]
    private static void TestAPF() {
        
    }
    private static void Patch_TestAPF(MethodDefinition md) 
    {
        var ilp = md.Body.GetILProcessor();
        ilp.Clear();
        ilp.Emit(MOpCodes.Ldsfld, typeof(Test2).GetField("instance", HReflectionHelper.All));
        ilp.Emit(MOpCodes.Ldc_I4_1);
        ilp.Emit(MOpCodes.Stfld, typeof(Test2).GetField("test", HReflectionHelper.All));
        ilp.Emit(MOpCodes.Ldsfld, typeof(Test2).GetField("instance", HReflectionHelper.All));
        ilp.Emit(MOpCodes.Callvirt, typeof(Test2).GetMethod("PrintTest", HReflectionHelper.All));
        ilp.Emit(MOpCodes.Ret);
    }
}
