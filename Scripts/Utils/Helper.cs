
namespace HKTool.Utils;

[ModuleDefine("HKTool.Helper", "0.1")]
public static class Helper
{
    private static MethodInfo M_clone = (MethodInfo)FindMethodBase("System.Object::MemberwiseClone")!;
    public static T MemberwiseClone<T>(this T self) => (T)M_clone.FastInvoke(self)!;
}
