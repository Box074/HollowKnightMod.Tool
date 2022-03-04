
namespace HKTool.PackMod;

static class PackModLoader
{
    public static Dictionary<Assembly, Dictionary<string, Assembly>> moduleMap = new();
    public static Assembly LoadModule(Assembly ass, string name)
    {
        var real = ass.GetRealAssembly();
        if(!moduleMap.TryGetValue(real, out var v))
        {
            v = new();
            moduleMap.Add(real, v);
        }
        if(v.TryGetValue(name, out var o)) return o;
        var moduleName = $"modules.{name.ToLower()}";
        var res = EmbeddedResHelper.GetBytes(real, moduleName, false);
        if(res == null) throw new DllNotFoundException();
        var pdbN = moduleName + ".pdb";
        var pdb = EmbeddedResHelper.GetBytes(real, pdbN, false);
        var module = pdb == null ? Assembly.Load(res) : Assembly.Load(res, pdb);
        v.Add(name, module);
        return module;
    }
}
