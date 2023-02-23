
namespace HKTool.Utils.Compile;

public static class FIL
{
    [PatchCaller(typeof(FIL), "PIL_SizeOf")]
    public static int SizeOf<T>() => SizeOf<T>();
    private static void PIL_SizeOf(MemberReference mr, MethodDefinition caller, Instruction il)
    {
        var m = (GenericInstanceMethod)mr;
        il.OpCode = MOpCodes.Sizeof;
        il.Operand = m.GenericArguments[0];
    }
    
}
