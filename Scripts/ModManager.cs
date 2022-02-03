
namespace HKTool;

static class ModManager
{
    public static List<ModBase> mods = new();
    public static void NewMod(ModBase mod)
    {
        if(mod is null) return;
        if(mods.Contains(mod)) return;
        mods.Add(mod);
        foreach(var v in mods.GetType().Assembly.GetTypes())
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
