
using static Modding.Logger;

namespace HKTool;
class DebugModsLoader
{
    public static List<Mod> DebugMods { get; } = new List<Mod>();

    public static void LoadMod(Assembly ass)
    {
        try
        {
            foreach (var type in ass.SafeGetTypes())
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
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var v in p)
        {
            var v2 = Path.GetFullPath(v);
            if (!File.Exists(v2)) continue;
            try
            {
                var asm = Assembly.LoadFile(v2);
                if(!allAssemblies.Contains(asm)) ass.Add(asm);
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

