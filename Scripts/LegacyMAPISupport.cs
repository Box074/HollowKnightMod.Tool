
namespace HKTool;

static class LegacyMAPISupport
{
    public static void Init()
    {
        if (ModBase.CurrentMAPIVersion < CompileInfo.SUPPORT_PRELOAD_PREFAB_VERSION)
        {
            HookEndpointManager.Modify(FindType("Modding.ModLoader")!.GetMethod("GetPreloads", HReflectionHelper.All), (ILContext context) =>
            {
                var cursor = new ILCursor(context);
                cursor.Goto(0);
                cursor.EmitDelegate(MAPI70_PreloadPrefab_GetPreload);
            });
            ModManager.onLoadMod += MAPI70_PreloadPrefab_InitMod;
        }
    }

    private static Dictionary<IMod, PreloadObject> preloadPrefabs = new();
    private static void MAPI70_PreloadPrefab_InitMod(ModInstance mi, bool isNoFirst, PreloadObject po)
    {
        if(isNoFirst) return;
        if(!preloadPrefabs.TryGetValue(mi.Mod!, out var v)) return;
        foreach(var v2 in v)
        {
            po[v2.Key] = v2.Value;
        }
    }
    private static void MAPI70_PreloadPrefab_GetPreload()
    {
        foreach(var mi in ModLoaderHelper.GetMods())
        {
            if(mi.Error is not null) continue;
            var mod = mi.Mod;
            if(mod is null or ModBase) continue;
            var preloads = mod.GetPreloadNames();
            foreach(var v in preloads)
            {
                if(v.Item1.StartsWith("sharedassets") || v.Item1 == "resources")
                {
                    HKToolMod.logger.LogWarn($"Preload prefab (\"{v.Item1}\",\"{v.Item2}\") request by mod \"{mod.GetName()}\"");
                    string sceneName;
                    if(v.Item1 == "resources")
                    {
                        sceneName = "resources";
                    }
                    else
                    {
                        if(!int.TryParse(v.Item1.Substring(12), out var sceneId)) continue;
                        if(sceneId >= ModBase.sceneNames.Length) continue;
                        sceneName = ModBase.sceneNames[sceneId];
                    }
                    var v2 = HKToolMod.Instance.assetpreloads.TryGetOrAddValue(sceneName, () => new());
                    v2.Add((v.Item2, typeof(GameObject), new Action<UObject?>(obj =>
                    {
                        preloadPrefabs.TryGetOrAddValue(mod, () => new()).TryGetOrAddValue(v.Item1, () => new())[v.Item2] = (GameObject)obj!;
                    })));
                }
            }
        }
    }
}
