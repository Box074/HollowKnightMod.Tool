
namespace HKTool;

public static class InitManager
{
    private static bool _isInit = false;
    public static void InstallInit()
    {
        var ass = ScriptingAssemblies.Load();
        ass.AddAssembly(typeof(InitManager).Assembly.Location);
        ass.Save();

        var init = RuntimeInitializeOnLoads.Load();
        if (init.root.Any(x => x.assemblyName == "HKTool" && x.nameSpace == "HKTool" && x.className == "InitManager" && x.methodName == nameof(CheckInit))) return;
        init.root.Add(new()
        {
            assemblyName = "HKTool",
            nameSpace = "HKTool",
            className = "InitManager",
            methodName = nameof(CheckInit)
        });
        init.Save();
    }
    public static void UninstallInit()
    {
        var ass = ScriptingAssemblies.Load();
        ass.Remove(typeof(InitManager).Assembly.Location);
        ass.Save();

        var init = RuntimeInitializeOnLoads.Load();
        init.root.RemoveAll(x => x.assemblyName == "HKTool");
        init.Save();
    }
    private static void ExperimentalFeat()
    {
        if(HKToolMod.settings.ExperimentalConfig.allow_start_without_steam)
        {
            On.Steamworks.SteamAPI.RestartAppIfNecessary += (orig, id) =>
            {
                if(id.m_AppId == 367520U) return false;
                return orig(id);
            };
        }
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void CheckInit()
    {
        if (_isInit) return;
        _isInit = true;

        ModResManager.Init();
        ModListMenuHelper.Init();
        ExperimentalFeat();

        On.HutongGames.PlayMaker.FsmStateAction.ctor += (orig, self) =>
        {
            orig(self);
            self.Reset();
        };

        IL.System.Reflection.Emit.ILGenerator.Emit_OpCode_Type += context =>
        {
            var cur = new ILCursor(context);
            cur.TryGotoNext(MoveType.After, x =>
            {
                if (x.OpCode != MOpCodes.Callvirt) return false;
                var m = (Mono.Cecil.MethodReference)x.Operand;
                return m.DeclaringType.FullName == "System.Type" && m.Name == "get_IsByRef";
            });
            cur.Emit(MOpCodes.Pop);
            cur.Emit(MOpCodes.Ldc_I4_0);
        };
    }
}

