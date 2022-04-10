
namespace HKTool.Modules;

public static class ModuleManager
{
    private static Dictionary<string, (Version ver, Type type)> modules = new();

    static ModuleManager()
    {
        foreach(var v in typeof(ModuleManager).Assembly.GetTypes())
        {
            var attr = v.GetCustomAttribute<ModuleDefineAttribute>();
            if(attr is not null)
            {
                Register(attr.name, attr.ver, v);
            }
        }
    }

    internal static void Register(string name, Version ver, Type type)
    {
        if(name is null) throw new ArgumentNullException(nameof(name));
        if(ver is null) throw new ArgumentNullException(nameof(ver));
        if(type is null) throw new ArgumentNullException(nameof(type));

        modules.Add(name.ToLower(), (ver, type));
    }
    public static bool HasModule(string name, Version? ver = null)
    {
        if(name is null) throw new ArgumentNullException(nameof(name));
        if(!modules.TryGetValue(name.ToLower(), out var v)) return false;
        if(ver is null) return true;
        return v.ver >= ver;
    }
    public static bool TryGetModule(string name, out Type? type, Version? ver = null)
    {
        if(name is null) throw new ArgumentNullException(nameof(name));
        type = null;
        if(!modules.TryGetValue(name.ToLower(), out var v)) return false;
        if(ver is null || v.ver >= ver)
        {
            type = v.type;
            return true;
        }
        return false;
    }
}
