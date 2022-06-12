
using Mono.Cecil;
using static Modding.Logger;

namespace HKTool;
class DebugModsLoader
{
    public static List<Mod> DebugMods { get; } = new List<Mod>();
    public static Dictionary<string, string> locationMap = new();

    static DebugModsLoader()
    {
        HookEndpointManager.Add(FindMethodBase("System.Reflection.Assembly::get_Location"),
            (Func<Assembly, string> orig, Assembly self) =>
            {
                if (locationMap.TryGetValue(self.FullName, out var p)) return p;
                return orig(self);
            });
    }
    private static byte[] ModifyAssembly(string path)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            AssemblyDefinition ass = AssemblyDefinition.ReadAssembly(path);
            ass.Write(stream);
            var b = stream.ToArray();
            locationMap.Add(ass.FullName, path);
            return b;
        }
    }

    public static void LoadMod(Assembly ass)
    {
        try
        {
            foreach (var type in ass.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Mod)))
                {
                    try
                    {
                        ConstructorInfo constructor = type.GetConstructor(new Type[0]);
                        if ((constructor?.Invoke(new object[0])) is Mod mod)
                        {
                            DebugMods.Add(mod);
                            ModLoaderHelper.AddModInstance(type, new()
                            {
                                Mod = mod,
                                Enabled = false,
                                Error = null,
                                Name = mod.GetName()
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        LogError(e);
                        ModLoaderHelper.AddModInstance(type, new()
                        {
                            Mod = null,
                            Enabled = false,
                            Error = ModErrorState.Construct,
                            Name = type.Name
                        });
                    }
                }
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    public static void LoadMods(List<string> p)
    {
        List<Assembly> ass = new List<Assembly>();
        foreach (var v in p)
        {
            var v2 = Path.GetFullPath(v);
            if (!File.Exists(v2)) continue;
            try
            {
                ass.Add(Assembly.Load(ModifyAssembly(v2), File.Exists(Path.ChangeExtension(v2, "pdb")) ? File.ReadAllBytes(Path.ChangeExtension(v2, "pdb")) : null));
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }
        foreach (var v in ass)
        {
            LoadMod(v);
        }
    }
}

