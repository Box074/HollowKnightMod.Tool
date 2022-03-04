
namespace HKTool;

static class ModManager
{
    public static List<ModBase> modsTable = new();
    public static Dictionary<Type, ModBase> instanceMap = new();
    public static List<(string, string)> modErrors = new();
    public static ReflectionObject RModLoader = new(DebugModsLoader.TModLoader);
    static ModManager()
    {
        HookEndpointManager.Add(
            RModLoader
                .GetObjectType()
                .GetMethod("UpdateModText", BindingFlags.NonPublic | BindingFlags.Static),
            (Action orig) =>
            {
                orig();
                if(modErrors.Count == 0) return;
                var vd = RModLoader.GetMemberData<ModVersionDraw>("modVersionDraw");
                var sb = new StringBuilder();
                sb.AppendLine(vd.drawString);
                sb.AppendLine();
                foreach(var v in modErrors)
                {
                    sb.Append(v.Item1);
                    sb.Append(" : ");
                    sb.AppendLine(v.Item2);
                }
                vd.drawString = sb.ToString();
            }
        );
    }
    public static void NewMod(ModBase mod, string name = null)
    {
        if(mod is null) return;
        if(modsTable.Contains(mod)) return;

        if(instanceMap.ContainsKey(mod.GetType()))
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
        foreach(var v in mod.GetType().Assembly.GetTypes())
        {
            if(v.GetCustomAttribute<AttachHeroControllerAttribute>() is not null)
            {
                HookManager.attachHeroController.Add(v);
            }
            if(v.GetCustomAttribute<AttachHealthManagerAttribute>() is not null)
            {
                HookManager.attachHealthManager.Add(v);
            }
        }
    }
}
