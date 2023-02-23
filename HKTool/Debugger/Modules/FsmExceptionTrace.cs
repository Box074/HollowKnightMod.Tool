
namespace HKTool.Debugger.Modules;

internal class FsmExceptionTrace : DebugModule
{
    private List<ILHook> hooks = null!;
    public override void OnEnable()
    {
        if (hooks == null)
        {
            hooks = new();
            var fsmThrowCatch = typeof(CompilerHelper).GetMethod(nameof(CompilerHelper.FsmThrowException));
            foreach (var v in typeof(FsmState).GetMethods().Where(x => x.Name.StartsWith("On") && !x.IsStatic))
            {
                if (v.Name == "OnEnter") continue;
                hooks.Add(new(v, (ILContext il) =>
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
                }));
            }
            hooks.Add(new(FindMethodBase("HutongGames.PlayMaker.FsmState::ActivateActions"), il =>
            {
                var call = il.Body.Instructions.First(x =>
                {
                    var mr = x.Operand as MethodReference;
                    if (mr == null) return false;
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
            }));
        }
        else
        {
            foreach (var v in hooks) v.Apply();
        }
    }
    public override void OnDisable()
    {
        foreach (var v in hooks) v.Undo();
    }
    public override bool CanRuntimeEnabled => true;
    public override bool CanRuntimeDisabled => true;
}
