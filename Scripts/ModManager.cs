
namespace HKTool;

static class ModManager
{
    public static bool modLoaded => ModLoaderHelper.modLoadState.HasFlag(ModLoadState.Loaded);
    public static List<ModBase> modsTable = new();
    public static Dictionary<Type, ModBase> instanceMap = new();
    public static List<(string, string)> modErrors = new();
    public static List<Type> skipMods = new();
    public static ReflectionObject RModLoader => ModLoaderHelper.RModLoader;
    public static Dictionary<Mod, Func<PreloadObject, bool>> hookInits = new();
    public static Dictionary<Mod, Func<List<(string, string)>, List<(string, string)>>> hookGetPreloads = new();
    static ModManager()
    {
        ModHooks.FinishedLoadingModsHook += () =>
        {
            skipMods.Clear();
            skipMods = null;
        };
        HookEndpointManager.Add
        (
            typeof(Type).GetMethod("GetConstructor", new Type[]
            {
                typeof(Type[])
            }),
            (Func<Type, Type[], ConstructorInfo> orig, Type self, Type[] types) =>
            {
                if (modLoaded || skipMods is null || !self.IsSubclassOf(typeof(ModBase))) return orig(self, types);
                if ((types?.Length ?? -1) == 0 && skipMods.Contains(self)) return null;
                else return orig(self, types);
            }
        );
        HookEndpointManager.Add(
            RModLoader
                .GetObjectType()
                .GetMethod("UpdateModText", BindingFlags.NonPublic | BindingFlags.Static),
            (Action orig) =>
            {
                orig();
                if (modErrors.Count == 0) return;
                var vd = RModLoader.GetMemberData<ModVersionDraw>("modVersionDraw");
                var sb = new StringBuilder();
                sb.AppendLine(vd.drawString);
                sb.AppendLine();
                foreach (var v in modErrors)
                {
                    sb.Append("<color=" + ModHooks.GlobalSettings.ConsoleSettings.ErrorColor + ">");
                    sb.Append(v.Item1);
                    sb.Append(" : ");
                    sb.Append(v.Item2);
                    sb.AppendLine("</color>");
                }
                vd.drawString = sb.ToString();
            }
        );
        HookEndpointManager.Add(
            RModLoader.GetObjectType().GetMethod("LoadMod", HReflectionHelper.All),
                (Action<object, bool, PreloadObject> orig,
                    object mod, bool updateVer, PreloadObject objs
                    ) =>
                    {
                        if(mod is null) return;
                        var mi = new ModInstance(mod);
                        if(mi.Error.HasValue) return;
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
                                if (!modLoaded)
                                {
                                    LoadModSelf(mi, updateVer, objs);
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                mi.Error = ModErrorState.Initialize;
                                mi.Write();
                                HKToolMod.logger.LogError(e);
                            }

                        }
                        else if(modLoaded)
                        {
                            orig(mod, updateVer, objs);
                        }
                    }
        );
        HookEndpointManager.Add(
            typeof(Mod).GetMethod("GetPreloadNames"),
            (Func<Mod, List<(string, string)>> orig, Mod self) =>
            {
                var list = orig(self);
                if (!hookGetPreloads.TryGetValue(self, out var mod)) return list;
                if (list is null) list = new();
                list = mod.Invoke(list);
                return list;
            }
        );
    }
    private static void LoadModSelf(ModInstance modInst, bool updateModText, PreloadObject objs)
    {

        if (modInst is not null && modInst.Enabled)
        {
            if (!modInst.Error.HasValue)
            {
                modInst.Enabled = true;
                modInst.Write();
            }
        }

        if (updateModText)
        {
            RModLoader.InvokeMethod("UpdateModText");
        }
    }
    public static void NewMod(ModBase mod, string name = null)
    {
        if (mod is null) return;
        if (modsTable.Contains(mod)) return;

        if (instanceMap.ContainsKey(mod.GetType()))
        {
            var err = "HKTool.Error.ModMultiInstance"
                .GetFormat(name ?? mod.GetType().Name, mod.GetType().Assembly.Location);
            mod.LogError(err);
            ModManager.modErrors.Add((name ?? mod.GetType().Name, "HKTool.ErrorShow.ModMultiInstance"
                .GetFormat(mod.GetType().Assembly.Location)));
            throw new InvalidOperationException(err);
        }

        instanceMap.Add(mod.GetType(), mod);
        modsTable.Add(mod);
        foreach (var v in mod.GetType().Assembly.GetTypes())
        {
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
