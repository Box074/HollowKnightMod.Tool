
namespace HKTool;
public static class DebugManager
{
    public static bool IsDebugMode => HKToolMod.IsDebugMode;
    public static bool IsDebug(Mod mod) => DebugModsLoader.DebugMods.Contains(mod);
    public static Mod[] DebugMods => DebugModsLoader.DebugMods.ToArray();
    public static int? DebugPort { get; private set; } = null;
    public static void Init()
    {
        List<string> debugFiles = new();
        Dictionary<string, string> libraryMods = new();
        var cmds = Environment.GetCommandLineArgs();
        bool isInputFile = false;
        foreach(var v in cmds)
        {
            if(v.Equals("--hktool-debug-mods", StringComparison.OrdinalIgnoreCase))
            {
                isInputFile = true;
                continue;
            }
            if(v.StartsWith("--hktool-debug-port=", StringComparison.OrdinalIgnoreCase))
            {
                if(int.TryParse(v.Split('=')[1], out var port))
                {
                    DebugPort = port;
                }
            }

            if(!v.StartsWith("--"))
            {
                if(isInputFile)
                {
                    var split = v.Split('=');
                    if(split.Length == 0)
                    {
                        debugFiles.Add(v);
                    }
                    else
                    {
                        libraryMods.Add(split[0], split[1]);
                    }
                    continue;
                }
            }
            else
            {
                isInputFile = false;
            }
        }
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var name = args.Name.Split(',')[0];
            if(!libraryMods.TryGetValue(name, out var path)) return null;
            if(!File.Exists(path)) return null;
            try
            {
                return Assembly.LoadFile(path);
            }
            catch(Exception)
            {
                return null;
            }
        };
        DebugModsLoader.LoadMods(debugFiles);
    }
}

