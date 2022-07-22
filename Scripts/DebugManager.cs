
namespace HKTool;
public static class DebugManager
{
    public static bool IsDebugMode => HKToolMod.IsDebugMode;
    public static bool IsDebug(Mod mod) => DebugModsLoader.DebugMods.Contains(mod);
    internal static Mod[] DebugMods => DebugModsLoader.DebugMods.ToArray();
    internal static int? DebugPort { get; private set; } = null;
    internal record FsmExcpetionInfo(string stateName, Fsm fsm, FsmStateAction action);
    internal static ConditionalWeakTable<Exception, FsmExcpetionInfo> fsmExcpetionInfo = new();
    internal static void Init()
    {
        List<string> debugFiles = new();
        List<Assembly> assemblies = new();
        Dictionary<string, string> libraryMods = new();
        var cmds = Environment.GetCommandLineArgs();
        bool isInputFile = false;
        foreach (var v in cmds.Skip(1))
        {
            HKToolMod.logger.Log("Parse Command: " + v);
            if (v.Equals("--hktool-debug-mods", StringComparison.OrdinalIgnoreCase))
            {
                isInputFile = true;
                continue;
            }
            if (v.StartsWith("--hktool-debug-port=", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(v.Split('=')[1], out var port))
                {
                    DebugPort = port;
                }
            }

            if (!v.StartsWith("--"))
            {
                if (isInputFile)
                {
                    var split = v.Split('=');
                    if (split.Length == 1)
                    {
                        HKToolMod.logger.Log("Debug Mod: " + split[0]);
                        debugFiles.Add(split[0]);
                    }
                    else
                    {
                        var p = split[1];
                        HKToolMod.logger.Log($"Library Mod({split[0]}): {p}");
                        libraryMods.Add(split[0], p);
                        if (File.Exists(p))
                        {
                            try
                            {
                                var asm = Assembly.LoadFile(p);
                            }
                            catch (Exception e)
                            {
                                HKToolMod.logger.LogError(e);
                            }
                        }
                    }
                    continue;
                }
            }
            else
            {
                isInputFile = false;
            }
        }
        foreach (var v in assemblies) DebugModsLoader.LoadMod(v);
        DebugModsLoader.LoadMods(debugFiles);

        var fsmThrowCatch = typeof(CompilerHelper).GetMethod(nameof(CompilerHelper.FsmThrowException));
        foreach (var v in typeof(FsmState).GetMethods().Where(x => x.Name.StartsWith("On") && !x.IsStatic))
        {
            if (v.Name == "OnEnter") continue;
            HookEndpointManager.Modify(v, (ILContext il) =>
            {
                var blts = il.Body.Instructions.First(x => x.OpCode == MOpCodes.Blt_S || x.OpCode == MOpCodes.Blt);
                var next = blts.Next;
                var ilp = il.IL;
                var leave = Instruction.Create(MOpCodes.Leave, next);
                var catchStart = Instruction.Create(MOpCodes.Nop);
                var catchEnd = Instruction.Create(MOpCodes.Leave, next);

                var exBlock = new Mono.Cecil.Cil.ExceptionHandler(ExceptionHandlerType.Catch);
                exBlock.CatchType = il.Import(typeof(Exception));
                exBlock.TryStart = ((ILLabel)blts.Operand).Target;
                exBlock.TryEnd = leave;
                exBlock.HandlerStart = catchStart;
                exBlock.HandlerEnd = catchEnd;
                il.Body.ExceptionHandlers.Add(exBlock);

                ilp.InsertAfter(blts, leave);
                ilp.InsertAfter(leave, catchStart);
                ilp.InsertAfter(catchStart, Instruction.Create(MOpCodes.Call, il.Import(fsmThrowCatch)));
                ilp.InsertBefore(next, catchEnd);
            });
        }
        IL.HutongGames.PlayMaker.FsmState.ActivateActions += il =>
        {
            var call = il.Body.Instructions.First(x => {
                var mr = x.Operand as MethodReference;
                if(mr == null) return false;
                return x.OpCode == MOpCodes.Callvirt && (mr.DeclaringType.Name == nameof(FsmStateAction)) && mr.Name == nameof(FsmStateAction.OnEnter);
            });
            var next = call.Next;
            var ilp = il.IL;
            var leave = Instruction.Create(MOpCodes.Leave, next);
            var catchStart = Instruction.Create(MOpCodes.Nop);
            var catchEnd = Instruction.Create(MOpCodes.Leave, next);

            var exBlock = new Mono.Cecil.Cil.ExceptionHandler(ExceptionHandlerType.Catch);
            exBlock.CatchType = il.Import(typeof(Exception));
            exBlock.TryStart = call;
            exBlock.TryEnd = leave;
            exBlock.HandlerStart = catchStart;
            exBlock.HandlerEnd = catchEnd;
            il.Body.ExceptionHandlers.Add(exBlock);

            ilp.InsertAfter(call, leave);
            ilp.InsertAfter(leave, catchStart);
            ilp.InsertAfter(catchStart, Instruction.Create(MOpCodes.Call, il.Import(fsmThrowCatch)));
            ilp.InsertBefore(next, catchEnd);
        };
    }
}

