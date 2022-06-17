
namespace HKTool.Runtime;

public static class CompilerHelper
{
    private static List<NativeDetour> hooks = new();
    public static void Hook_Add(Delegate hook, RuntimeMethodHandle method) => HookEndpointManager.Add(MethodBase.GetMethodFromHandle(method), hook);
    public static void Hook_Remove(Delegate hook, RuntimeMethodHandle method) => HookEndpointManager.Remove(MethodBase.GetMethodFromHandle(method), hook);
    public static void Hook_Modify(Delegate hook, RuntimeMethodHandle method) => HookEndpointManager.Modify(MethodBase.GetMethodFromHandle(method), hook);
    public static void Hook_Unmodify(Delegate hook, RuntimeMethodHandle method) => HookEndpointManager.Unmodify(MethodBase.GetMethodFromHandle(method), hook);
    public static void Prepare_Caller(RuntimeMethodHandle target, RuntimeMethodHandle self)
    {
        hooks.Add(new(MethodBase.GetMethodFromHandle(self), MethodBase.GetMethodFromHandle(target)));
    }
}
