
namespace HKTool;

public static class InitManager
{
    private static bool _isInit = false;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void CheckInit()
    {
        if(ModBase.CurrentMAPIVersion < 72) return;
        if (_isInit) return;
        _isInit = true;

        ModResManager.Init();
        ModListMenuHelper.Init();
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

