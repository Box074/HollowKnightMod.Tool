
namespace HKTool.MAPI.Loader;

[Obsolete]
public static class ModLoaderHelper
{
    public static ModLoadState modLoadState => (ModLoadState)ModLoaderR.LoadState;
    public static bool HasModLoadState => true;
    public static Type TModLoader = ReflectionHelperEx.FindType("Modding.ModLoader")!;
    public static void AddModInstance(Type type, ModInstance mi)
    {
        if(modLoadState.HasFlag(ModLoadState.Loaded)) throw new InvalidOperationException();
        ModLoaderR.TryAddModInstance(type, (ModInstanceR)mi.Get());
    }
    public static IReadOnlyList<ModInstance> GetMods()
    {
        return ModLoaderR.ModInstances.Select(x => new ModInstance(x)).ToList().AsReadOnly();
    }
}
