
namespace HKTool;

static class ModManager
{
    public static readonly bool SupportPreloadAssets = FindType("Modding.ModLoader")!.GetMethod("LoadMod", HReflectionHelper.All).GetParameters().Length == 4;
    public static bool modLoaded => ModLoaderHelper.modLoadState.HasFlag(ModLoadState.Loaded);
    public static List<ModBase> mods = new();
    public static Dictionary<Type, ModBase> instanceMap = new();
    public static List<(string, string)> modErrors = new();
    public static Dictionary<Mod, Func<PreloadObject, bool>> hookInits = new();
    public static Dictionary<Mod, Func<List<(string, string)>, List<(string, string)>>> hookGetPreloads = new();
    public static event OnLoadModHandler onLoadMod = null!;
    public delegate void OnLoadModHandler(ModInstance mi,ref bool updateVer, ref PreloadObject preloads);
    private static Ref<ModVersionDraw> ref_ModVersionDraw = GetFieldRefPointer(null, FindFieldInfo("Modding.ModLoader::modVersionDraw"));
    static ModManager()
    {
        
        On.Modding.ModLoader.UpdateModText += (orig) =>
        {
            orig();
            var vd = ref_ModVersionDraw.Value;
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
        };
        if(ModBase.CurrentMAPIVersion < 72) return;
        HookEndpointManager.Add(
            FindType("Modding.ModLoader")!.GetMethod("LoadMod", HReflectionHelper.All),
                HookLoadMod
        );
        On.Modding.Mod.GetPreloadNames += (orig, self) =>
            {
                var list = orig(self);
                if (!hookGetPreloads.TryGetValue(self, out var mod)) return list;
                if (list is null) list = new();
                list = mod.Invoke(list);
                return list;
            };
    }
    private static void HookLoadMod(Action<object, bool, PreloadObject> orig,
                    object mod, bool updateVer, PreloadObject objs)
    {
        if (mod is null) return;
        var mi = new ModInstance(mod);
        try
        {
            onLoadMod?.Invoke(mi, ref updateVer, ref objs);
        }
        catch (Exception e)
        {
            HKToolMod.logger.LogError(e);
        }
        if (mi.Error.HasValue) return;
        try
        {
            var ms = mi.Mod;
            if (ms is IHKToolMod hmod && !modLoaded)
            {
                hmod.HookInit(objs);
            }
            if (!modLoaded)
            {
                orig(mod, updateVer, objs);
            }
            if (ms is ITogglableModBase thmod)
            {
                try
                {
                    thmod.OnLoad();
                    if (modLoaded)
                    {
                        LoadModSelf(mi, updateVer);
                        return;
                    }
                }
                catch (Exception e)
                {
                    mi.Error = ModErrorState.Initialize;
                    HKToolMod.logger.LogError(e);
                }

            }
            else if (modLoaded)
            {
                orig(mod, updateVer, objs);
            }
        }
        catch (Exception e)
        {
            mi.Error = ModErrorState.Initialize;
            HKToolMod.logger.LogError(e);
        }
    }
    private static void LoadModSelf(ModInstance modInst, bool updateModText)
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
            ((MethodInfo)FindMethodBase("Modding.ModLoader::UpdateModText")).FastInvoke(null);
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
            ModManager.modErrors.Add((name ?? mod.GetType().Name, "HKTool.ErrorShow.ModMultiInstance"
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
