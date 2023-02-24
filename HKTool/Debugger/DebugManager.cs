
namespace HKTool;
public static class DebugManager
{
    public static bool IsDebugMode => HKToolMod2.IsDebugMode;
    public static bool IsDebug(Mod mod) => DebugModsLoader.DebugMods.Contains(mod);
    internal static Mod[] DebugMods => DebugModsLoader.DebugMods.ToArray();
    internal static int? DebugPort { get; private set; } = null;
    internal record FsmExcpetionInfo(string stateName, Fsm fsm, FsmStateAction action);
    internal static ConditionalWeakTable<Exception, FsmExcpetionInfo> fsmExcpetionInfo = new();
    internal static List<DebugModule> modules = new();
    internal static void Init()
    {
        foreach(var v in typeof(DebugModule).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(DebugModule))))
        {
            var module = (DebugModule)Activator.CreateInstance(v);
            modules.Add(module);
            if(!HKToolSettings.TryLoad().DebugConfig.disabledModules.Contains(module.ModuleName))
            {
                module.OnEnable();
            }
        }

        List<string> debugFiles = new();
        List<Assembly> assemblies = new();
        Dictionary<string, string> libraryMods = new();
        var cmds = Environment.GetCommandLineArgs();
        bool isInputFile = false;
        foreach (var v in cmds.Skip(1))
        {
            HKToolMod2.logger.Log("Parse Command: " + v);
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
                        HKToolMod2.logger.Log("Debug Mod: " + split[0]);
                        debugFiles.Add(split[0]);
                    }
                    else
                    {
                        var p = split[1];
                        HKToolMod2.logger.Log($"Library Mod({split[0]}): {p}");
                        libraryMods.Add(split[0], p);
                        if (File.Exists(p))
                        {
                            try
                            {
                                var asm = Assembly.LoadFile(p);
                                assemblies.Add(asm);
                            }
                            catch (Exception e)
                            {
                                HKToolMod2.logger.LogError(e);
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
        foreach (var v in assemblies) DebugModsLoader.LoadMod(v);
        DebugModsLoader.LoadMods(debugFiles);

        

        IEnumerator HotKey()
        {
            while (true)
            {
                yield return null;
                if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.RightAlt) && Input.GetKey(KeyCode.Tab))
                {
                    USceneManager.LoadScene("Quit_To_Menu", LoadSceneMode.Single);
                }
            }
        }
        HotKey().StartCoroutine().Start();
    }

    
}

