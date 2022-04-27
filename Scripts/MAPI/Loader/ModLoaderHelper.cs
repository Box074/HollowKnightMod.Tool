
namespace HKTool.MAPI.Loader;

[ModuleDefine("HKTool.ModLoaderHelper", "0.1")]
public static class ModLoaderHelper
{
    public static ModLoadState modLoadState
    {
        get
        {
            if(HasModLoadState)
            {
                return (ModLoadState?)(RModLoader["LoadState"]?.As<IConvertible>())?.ToInt32(null) ?? throw new NullReferenceException();
            }
            else
            {
                ModLoadState flags = ModLoadState.Started;
                if(RModLoader["Loaded"]?.As<bool>() ?? false) flags |= ModLoadState.Loaded;
                if(RModLoader["Preloaded"]?.As<bool>() ?? false) flags |= ModLoadState.Preloaded;
                return flags;
            }
        }
    }
    private static Lazy<bool> _hasModLoadState = 
        new(() => RModLoader?.GetObjectType()?.GetNestedType("ModLoadState") is not null);
    public static bool HasModLoadState => _hasModLoadState.Value;
    public static Type TModLoader = FindType("Modding.ModLoader")!;
    public readonly static ReflectionObject RModLoader = new(TModLoader);
    
    public static MethodInfo MAddModInstance = 
        TModLoader.GetMethod("TryAddModInstance", HReflectionHelper.All) ?? TModLoader.GetMethod("AddModInstance", HReflectionHelper.All);
    public static void AddModInstance(Type type, ModInstance mi)
    {
        if(modLoadState.HasFlag(ModLoadState.Loaded)) throw new InvalidOperationException();
        MAddModInstance.FastInvoke(null, type, mi.Get());
    }
}
