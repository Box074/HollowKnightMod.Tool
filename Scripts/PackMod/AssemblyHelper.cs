
namespace HKTool.PackMod;

public static class AssemblyHelper
{
    internal static Dictionary<Assembly, Assembly> packMap = new();
    public static Assembly GetRealAssembly(this Assembly ass)
    {
        if(packMap.TryGetValue(ass, out var v)) return v;
        return ass;
    }
}
