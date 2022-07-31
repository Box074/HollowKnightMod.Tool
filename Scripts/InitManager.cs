
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
        if (HKToolMod.settings.ExperimentalConfig.allow_start_without_steam)
        {
            On.Steamworks.SteamAPI.RestartAppIfNecessary += (orig, id) =>
            {
                if (id.m_AppId == 367520U) return false;
                return orig(id);
            };
        }
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void CheckInit()
    {
        if(ModBase.CurrentMAPIVersion < 72) return;
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

        BuildFakeHK();
    }

    private static void BuildFakeHK()
    {
        //HKTool.FakeHK
        var fhkp = Path.Combine(Path.GetDirectoryName(typeof(InitManager).Assembly.Location), "HKTool.FakeHK.dll");
        if (!File.Exists(fhkp))
        {
            var hkf = AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name == "Assembly-CSharp");

            using (AssemblyDefinition def = AssemblyDefinition.CreateAssembly(new("HKTool.FakeHK", new()), "HKTool.FakeHK", ModuleKind.Dll))
            {
                
                foreach (var v in hkf.GetTypes())
                {
                    var type = def.MainModule.ImportReference(v);
                    var attr = new CustomAttribute(def.MainModule.ImportReference(typeof(TypeForwardedToAttribute).GetConstructors()[0]));
                    attr.ConstructorArguments.Add(new(def.MainModule.ImportReference(typeof(Type)), type));
                    def.CustomAttributes.Add(attr);
                    def.MainModule.ExportedTypes.Add(new(type.Namespace, type.Name, def.MainModule, type.Scope));
                }
                def.Write(fhkp);
            }
        }
        Assembly.LoadFile(fhkp);
    }
}

