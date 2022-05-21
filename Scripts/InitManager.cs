
namespace HKTool;

public static class InitManager
{
    private static bool _isInit = false;
    public static void CheckInit()
    {
        if (_isInit) return;
        _isInit = true;

        ModResManager.Init();
        ModListMenuHelper.Init();

        On.HutongGames.PlayMaker.FsmStateAction.ctor += (orig, self) =>
        {
            orig(self);
            self.Reset();
        };

        HookEndpointManager.Modify(typeof(ILGenerator).GetMethod("Emit", new Type[]{
            typeof(OpCode),
            typeof(Type)
        }), (ILContext context) =>
        {
            var cur = new ILCursor(context);
            cur.TryGotoNext(MoveType.After, x =>
            {
                if(x.OpCode != MOpCodes.Callvirt) return false;
                var m = (Mono.Cecil.MethodReference)x.Operand;
                return m.DeclaringType.FullName == "System.Type" && m.Name =="get_IsByRef";
                });
            cur.Emit(MOpCodes.Pop);
            cur.Emit(MOpCodes.Ldc_I4_0);
        });
    }
}

