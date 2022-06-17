
namespace HKTool.Test;

class ILTest
{
    public static int test1 = 0;
    public static int test2 = 1;
    public static string test3 = "";
    public static FieldInfo mlf = FindFieldInfo("Modding.ModLoader+ModInstance::Mod")!;
    public static Type ml = FindType("Modding.ModLoader+ModInstance")!;
    public static FieldInfo mlf1 = FindFieldInfo<PlayerData>("atBench");
    public static Ref<string> tr1 = GetRefPointer(ref test3);
    public static void Test()
    {
        GetFieldRef<bool>(GameManager.instance, "GameManager::<IsInSceneTransition>k__BackingField") = true;
        tr1.Value = "Hello,World!";
        On.HeroController.GetCState += (orig, self, name) => orig(self, name);
        GetFieldRef<bool, GameManager>(GameManager.instance, "<IsInSceneTransition>k__BackingField") = false;
        StaticHeroController.private__instance().private_audioCtrl().private_m_CachedPtr();
        HeroController.instance.CanDash();
    }
}
