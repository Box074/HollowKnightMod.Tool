
using HKTool.FSM.CSFsmEx;
using HKTool.FSM.CSFsmEx.Compiler;

namespace HKTool.Test;

static class TestManager
{

    public static (string name, Action onClick)[] tests = new(string name, Action onClick)[]{
        ("CSFsm Host Compiler", () => {
            var c = new CSFsmCompiler();
            var md = AssemblyDefinition.CreateAssembly(new("Test1", new()), "Test1", ModuleKind.Dll).MainModule;
            var mt = new TypeDefinition("", "TestCompiler", Mono.Cecil.TypeAttributes.AutoLayout);
            md.Types.Add(mt);
            var m = new MethodDefinition("Build", Mono.Cecil.MethodAttributes.Static, md.TypeSystem.Void);
            m.Parameters.Add(new(md.TypeSystem.Object));
            mt.Methods.Add(m);
            c.Compile(typeof(TCSFsmCompiler), m);
            md.Assembly.Write(@"C:\Users\29676\Desktop\A.dll");
        }),
        ("CSFsm Host Test1", () =>
        {
            var go = new GameObject();
            CSFsmProxy.Attach<TCSFsmCompiler>(go, out _, "Test_Start1");
        })
    };
}
