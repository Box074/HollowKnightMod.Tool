
namespace HKTool.Test;

class ILTest
{
    public static int test1 = 0;
    public static int test2 = 1;
    public static FieldInfo test1Field = GetFieldSelf("test1")!;
    public static FieldInfo mlf = FindFieldInfo("Modding.ModLoader+ModInstance::Mod")!;
    public static Type ml = FindType("Modding.ModLoader+ModInstance")!;
    public static void Test()
    {
        GetMethodSelf(".ctor");
        GetMethodSelf(".cctor");
        GetFieldRef<bool>(GameManager.instance, "GameManager::<IsInSceneTransition>k__BackingField") = true;
    }
}
