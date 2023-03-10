
namespace HKTool;

static class ModManager
{
    public static readonly bool SupportPreloadAssets = true;
    public static bool modLoaded => ModLoaderR.LoadState.HasFlag(ModLoadStateR.Loaded);
    public static List<ModBase> mods = new();
    public static Dictionary<Type, ModBase> instanceMap = new();
    public static List<(string, string)> modErrors = new();
    public static Dictionary<Mod, Func<PreloadObject, bool>> hookInits = new();
    public static Dictionary<Mod, Func<List<(string, string)>, List<(string, string)>>> hookGetPreloads = new();
    public static event OnLoadModHandler onLoadMod = null!;
    public delegate void OnLoadModHandler(ModInstanceR mi,ref bool updateVer, ref PreloadObject preloads);
    static ModManager()
    {
        On.Modding.ModLoader.UpdateModText += ModLoader_UpdateModText;
        On.Modding.ModLoader.LoadMod += HookLoadMod;
        On.Modding.Mod.GetPreloadNames += Mod_GetPreloadNames;
    }

    private static List<(string, string)> Mod_GetPreloadNames(On.Modding.Mod.orig_GetPreloadNames orig, Mod self)
    {
        var list = orig(self);
        if (!hookGetPreloads.TryGetValue(self, out var mod)) return list;
        if (list is null) list = new();
        list = mod.Invoke(list);
        return list;
    }

    private static void ModLoader_UpdateModText(On.Modding.ModLoader.orig_UpdateModText orig)
    {
        orig();
        var vd = ModLoaderR.modVersionDraw;
        var ds = vd?.drawString ?? "";
        var lines = ds.Split('\n');
        var sb = new StringBuilder();
        var displayNames = mods.Select(x => (x.GetName().Trim(), x.DisplayName.Trim())).Where(x => x.Item1 != x.Item2)
            .ToDictionary(x => x.Item1, x => x.Item2);
        foreach (var v in lines)
        {
            int pos = v.IndexOf(':');
            if (pos == -1)
            {
                sb.AppendLine(v);
                continue;
            }
            var name = v.Substring(0, pos).Trim();
            if (displayNames.TryGetValue(name, out var displayName))
            {
                sb.AppendLine(displayName + " " + v.Substring(pos));
            }
            else
            {
                sb.AppendLine(v);
            }
        }
        if (modErrors.Count > 0)
        {
            sb.AppendLine();
            foreach (var v in modErrors)
            {
                sb.Append("<color=" + ModHooks.GlobalSettings.ConsoleSettings.ErrorColor + ">");
                sb.Append(v.Item1);
                sb.Append(" : ");
                sb.Append(v.Item2);
                sb.AppendLine("</color>");
            }
        }
        if (vd is not null) vd.drawString = sb.ToString();
    }

    private static void HookLoadMod(On.Modding.ModLoader.orig_LoadMod orig,
        object mod, bool updateModText, PreloadObject preloadedObjects)
    {
        if (mod is null) return;
        var mi = (ModInstanceR)mod;
        try
        {
            onLoadMod?.Invoke(mi, ref updateModText, ref preloadedObjects);
        }
        catch (Exception e)
        {
            HKToolMod2.logger.LogError(e);
        }
        if (mi.Error.HasValue) return;
        try
        {
            var ms = mi.Mod;
            
            if (!modLoaded)
            {
                if (ms is IHKToolMod hmod)
                {
                    hmod.HookInit(preloadedObjects);
                }
                orig(mod, updateModText, preloadedObjects);
            }
            if (ms is ITogglableModBase thmod)
            {
                try
                {
                    thmod.OnLoad();
                    if (modLoaded)
                    {
                        LoadModSelf(mi, updateModText);
                        return;
                    }
                }
                catch (Exception e)
                {
                    mi.Error = ModErrorStateR.Initialize;
                    HKToolMod2.logger.LogError(e);
                }

            }
            else if (modLoaded)
            {
                orig(mod, updateModText, preloadedObjects);
            }
        }
        catch (Exception e)
        {
            mi.Error = ModErrorStateR.Initialize;
            HKToolMod2.logger.LogError(e);
        }
    }
    private static void LoadModSelf(ModInstanceR modInst, bool updateModText)
    {

        if (modInst is not null && !modInst.Enabled)
        {
            if (!modInst.Error.HasValue)
            {
                modInst.Enabled = true;
            }
        }

        if (updateModText)
        {
            ModLoaderR.UpdateModText();
        }
    }
    public static void NewMod(ModBase mod, string? name = null)
    {
        if (mod is null) return;
        if (mods.Contains(mod)) return;

        if (instanceMap.ContainsKey(mod.GetType()))
        {
            var err = "HKTool.Error.ModMultiInstance"
                .LocalizeFormat(name ?? mod.GetType().Name, mod.GetType().Assembly.Location);
            mod.LogError(err);
            modErrors.Add((name ?? mod.GetType().Name, "HKTool.ErrorShow.ModMultiInstance"
                .LocalizeFormat(mod.GetType().Assembly.Location)));
            throw new InvalidOperationException(err);
        }

        instanceMap.Add(mod.GetType(), mod);
        mods.Add(mod);
        foreach (var v in mod.GetType().Assembly.SafeGetTypes())
        {
            if(v is null) continue;
            if (v.GetCustomAttribute<AttachHeroControllerAttribute>() is not null)
            {
                HookManager.attachHeroController.Add(v);
            }
            if (v.GetCustomAttribute<AttachHealthManagerAttribute>() is not null)
            {
                HookManager.attachHealthManager.Add(v);
            }
        }
    }
}
