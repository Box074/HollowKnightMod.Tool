
namespace HKTool.Utils.Compile;

static class InternalPatcher
{
    public static MethodBase GetFieldFromHandle = typeof(FieldInfo)
        .GetMethod("GetFieldFromHandle", new Type[] { typeof(RuntimeFieldHandle) });
    public static MethodBase GetMethodFromHandle = typeof(MethodBase)
        .GetMethod("GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle) });
    public static MethodBase GetTypeFromHandle = typeof(Type)
        .GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });

    public static void Patch_Nop(MemberReference mr, MethodDefinition caller, Instruction il)
    {
        il.OpCode = MOpCodes.Nop;
    }
    public static void Patch_Ldarg0(MemberReference mr, MethodDefinition caller, Instruction il)
    {
        il.OpCode = MOpCodes.Ldarg_0;
    }
    public static void Patch_GetFieldRef(MemberReference mr, MethodDefinition caller, Instruction il)
    {
        var lastLdstr = il.Previous;
        if (lastLdstr.OpCode != MOpCodes.Ldstr) return;
        var s = (string)lastLdstr.Operand;
        var ilp = caller.Body.GetILProcessor();
        var tn = s.Substring(0, s.IndexOf(':'));
        var fn = s.Substring(s.LastIndexOf(':') + 1);
        var type = FindTypeEx(tn, caller.Module);
        if (type == null) return;
        var field = type.Fields.FirstOrDefault(x => x.Name == fn);
        if (field == null) return;

        lastLdstr.OpCode = MOpCodes.Ldtoken;
        lastLdstr.Operand = caller.Module.ImportReference(field);
        ilp.InsertAfter(lastLdstr, Instruction.Create(MOpCodes.Call, caller.Module.ImportReference(
            GetFieldFromHandle
            )));

        ilp.InsertBefore(il, Instruction.Create(MOpCodes.Call, caller.Module.ImportReference(
            typeof(ReflectionHelperEx).GetMethod("GetFieldRefPointer")
            )));
        il.OpCode = MOpCodes.Nop;
        il.Operand = caller.Module.ImportReference(new ByReferenceType(field.FieldType));
    }
    public static void Patch_FindMethodBase(MemberReference mr, MethodDefinition caller, Instruction il)
    {
        var lastLdstr = il.Previous;
        if (lastLdstr.OpCode != MOpCodes.Ldstr) return;
        var s = (string)lastLdstr.Operand;
        var tn = s.Substring(0, s.IndexOf(':'));
        var fn = s.Substring(s.LastIndexOf(':') + 1);
        var type = FindTypeEx(tn, caller.Module);
        if (type == null) return;
        var method = type.Methods.FirstOrDefault(x => x.Name == fn);
        if (method == null) return;
        lastLdstr.OpCode = MOpCodes.Ldtoken;
        lastLdstr.Operand = caller.Module.ImportReference(method);
        il.Operand = caller.Module.ImportReference(
            GetMethodFromHandle
            );
    }
    public static void Patch_FindFieldInfo(MemberReference mr, MethodDefinition caller, Instruction il)
    {
        var lastLdstr = il.Previous;
        if (lastLdstr.OpCode != MOpCodes.Ldstr) return;
        var s = (string)lastLdstr.Operand;
        var tn = s.Substring(0, s.IndexOf(':'));
        var fn = s.Substring(s.LastIndexOf(':') + 1);
        var type = FindTypeEx(tn, caller.Module);
        if (type == null) return;
        var field = type.Fields.FirstOrDefault(x => x.Name == fn);
        if (field == null) return;
        lastLdstr.OpCode = MOpCodes.Ldtoken;
        lastLdstr.Operand = caller.Module.ImportReference(field);
        il.Operand = caller.Module.ImportReference(
            GetFieldFromHandle
            );
    }
    public static void Patch_FindType(MemberReference mr, MethodDefinition caller, Instruction il)
    {
        var lastLdstr = il.Previous;
        if (lastLdstr.OpCode != MOpCodes.Ldstr) return;
        var s = (string)lastLdstr.Operand;
        var type = FindTypeEx(s, caller.Module);
        if (type == null) return;
        lastLdstr.OpCode = MOpCodes.Ldtoken;
        lastLdstr.Operand = caller.Module.ImportReference(type);
        il.Operand = caller.Module.ImportReference(
            GetTypeFromHandle
            );
    }

    public static TypeDefinition? FindType(string name, ModuleDefinition md)
    {
        if(md is null) return null;
        foreach (var v in md.Types)
        {
            if (v.FullName == name) return v;
        }
        foreach (var v in md.AssemblyReferences)
        {
            var t = md.AssemblyResolver.Resolve(v)
            ?.MainModule.Types.FirstOrDefault(x => x.FullName == name);
            if (t != null) return t;
        }
        return null;
    }
    public static TypeDefinition? FindTypeEx(string name, ModuleDefinition md)
    {
        if(md is null) return null;
        var parts = name.Split('+');
        var parent = FindType(parts[0], md);
        if (parent == null) return null;
        for (int a = 1; a < parts.Length; a++)
        {
            var n = parts[a];
            var t = parent.NestedTypes.FirstOrDefault(x => x.Name == n);
            if (t == null) return null;
            parent = t;
        }
        return parent;
    }
}
