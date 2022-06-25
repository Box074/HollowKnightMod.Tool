
namespace HKTool;
public static class DebugManager
{
    public static bool IsDebugMode => HKToolMod.IsDebugMode;
    public static bool IsDebug(Mod mod) => DebugModsLoader.DebugMods.Contains(mod);
    public static Mod[] DebugMods => DebugModsLoader.DebugMods.ToArray();
    public static int? DebugPort { get; private set; } = null;
    public static void Init()
    {
        List<string> files = new();
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
                    files.Add(v);
                    continue;
                }
            }
            else
            {
                isInputFile = false;
            }
        }
        DebugModsLoader.LoadMods(files);
    }
}

