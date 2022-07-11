
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
        List<Assembly> assemblies = new();
        Dictionary<string, string> libraryMods = new();
        var cmds = Environment.GetCommandLineArgs();
        bool isInputFile = false;
        foreach (var v in cmds.Skip(1))
        {
            HKToolMod.logger.Log("Parse Command: " + v);
            if (v.Equals("--hktool-debug-mods", StringComparison.OrdinalIgnoreCase))
            {
                isInputFile = true;
                continue;
            }
            if (v.StartsWith("--hktool-debug-port=", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(v.Split('=')[1], out var port))
                {
                    DebugPort = port;
                }
            }

            if (!v.StartsWith("--"))
            {
                if (isInputFile)
                {
                    var split = v.Split('=');
                    if (split.Length == 1)
                    {
                        HKToolMod.logger.Log("Debug Mod: " + split[0]);
                        debugFiles.Add(split[0]);
                    }
                    else
                    {
                        var p = split[1];
                        HKToolMod.logger.Log($"Library Mod({split[0]}): {p}");
                        libraryMods.Add(split[0], p);
                        if (File.Exists(p))
                        {
                            try
                            {
                                var asm = Assembly.LoadFile(p);
                            }
                            catch (Exception e)
                            {
                                HKToolMod.logger.LogError(e);
                            }
                        }
                    }
                    continue;
                }
            }
            else
            {
                isInputFile = false;
            }
        }
        foreach(var v in assemblies) DebugModsLoader.LoadMod(v);
        DebugModsLoader.LoadMods(debugFiles);
    }
}

