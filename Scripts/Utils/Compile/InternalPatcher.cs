
namespace HKTool.Utils.Compile;

static class InternalPatcher
{
    public static Dictionary<string, FieldDefinition> refCache = new();
    public static Dictionary<string, MethodDefinition> callerCache = new();
    public static TypeDefinition? refCacheType;
    public static TypeDefinition? callerCacheType;
    public static MethodBase GetFieldFromHandle = typeof(FieldInfo)
        .GetMethod("GetFieldFromHandle", new Type[] { typeof(RuntimeFieldHandle) });
    public static MethodBase GetMethodFromHandle = typeof(MethodBase)
        .GetMethod("GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle) });
    public static MethodBase GetTypeFromHandle = typeof(Type)
        .GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });

    public static FieldReference GetRefCache(string name, ModuleDefinition module)
    {
        if(refCacheType is null)
        {
            refCacheType = new(null, "<FieldRefCache>", Mono.Cecil.TypeAttributes.NotPublic | Mono.Cecil.TypeAttributes.Sealed, module.TypeSystem.Object);
            refCacheType.IsClass = true;
            module.Types.Add(refCacheType);
        }

        return refCache.TryGetOrAddValue(name, () => {
            var fd = new FieldDefinition("R_" + refCache.Count, Mono.Cecil.FieldAttributes.Assembly | Mono.Cecil.FieldAttributes.Static,
                module.ImportReference(typeof(RT_GetFieldPtr)));
            refCacheType.Fields.Add(fd);
            return fd;
        });
    }
    public static MethodReference GetRefCache(MethodReference method, ModuleDefinition module)
    {
        if(callerCacheType is null)
        {
            callerCacheType = new(null, "<CallerCache>", Mono.Cecil.TypeAttributes.NotPublic | Mono.Cecil.TypeAttributes.Sealed, module.TypeSystem.Object);
            callerCacheType.IsClass = true;
            module.Types.Add(callerCacheType);
        }

        return callerCache.TryGetOrAddValue(method.FullName, () => {
            var md = new MethodDefinition(method.Name + "|" + callerCache.Count, Mono.Cecil.MethodAttributes.Assembly | Mono.Cecil.MethodAttributes.Static,
                module.ImportReference(method.ReturnType));
            var body = md.Body = new(md);
            var ilp = body.GetILProcessor();
            ilp.Emit(MOpCodes.Ldtoken, module.ImportReference(method));
            ilp.Emit(MOpCodes.Ldtoken, md);
            ilp.Emit(MOpCodes.Call, module.ImportReference(typeof(CompilerHelper).GetMethod(nameof(CompilerHelper.Prepare_Caller))));
            int id = 0;
            if(!method.Resolve().IsStatic)
            {
                md.Parameters.Add(new(module.ImportReference(method.DeclaringType)));
                ilp.Emit(MOpCodes.Ldarg, id++);
            }
            foreach(var v in method.Resolve().Parameters)
            {
                md.Parameters.Add(new(module.ImportReference(v.ParameterType)));
                ilp.Emit(MOpCodes.Ldarg, id++);
            }
            ilp.Emit(MOpCodes.Call, md);
            ilp.Emit(MOpCodes.Ret);
            callerCacheType.Methods.Add(md);
            return md;
        });
    }
    public static void Patch_PrivateMethodCaller(MemberReference mr, MethodDefinition caller, Instruction il)
    {
        var ilp = caller.Body.GetILProcessor();
        var md = ((MethodReference)mr).Resolve();
        var method = (MethodReference) md.Body.Instructions[0].Operand;
        il.OpCode = MOpCodes.Call;
        il.Operand = GetRefCache(method, caller.Module);
    }
    public static void Patch_RefHelperEx(MemberReference mr, MethodDefinition caller, Instruction il)
    {
        var ilp = caller.Body.GetILProcessor();
        var md = ((MethodReference)mr).Resolve();
        var field = (FieldReference) md.Body.Instructions[0].Operand;

        ilp.InsertAfter(il, Instruction.Create(MOpCodes.Call, caller.Module.ImportReference(
            typeof(ReflectionHelperEx).GetMethod("GetFieldRefPointerEx")
            )));
        ilp.InsertAfter(il, Instruction.Create(MOpCodes.Ldsflda, GetRefCache(field.FullName, caller.Module)));
        ilp.InsertAfter(il, Instruction.Create(MOpCodes.Call, caller.Module.ImportReference(
            GetFieldFromHandle
            )));
        ilp.InsertAfter(il, Instruction.Create(MOpCodes.Ldtoken, caller.Module.ImportReference(field)));
        il.OpCode = field.Resolve().IsStatic ? MOpCodes.Ldnull : MOpCodes.Nop;
        il.Operand = null;
    }

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

        ilp.InsertBefore(il, Instruction.Create(MOpCodes.Ldsflda, GetRefCache(field.FullName, caller.Module)));
        il.OpCode = MOpCodes.Call;
        il.Operand = caller.Module.ImportReference(
            typeof(ReflectionHelperEx).GetMethod("GetFieldRefPointerEx")
            );
    }
    public static void Patch_GetFieldRefEx(MemberReference mr, MethodDefinition caller, Instruction il)
    {
        var lastLdstr = il.Previous;
        if (lastLdstr.OpCode != MOpCodes.Ldstr) return;
        var s = (string)lastLdstr.Operand;
        var ilp = caller.Body.GetILProcessor();
        var type = ((GenericInstanceMethod)mr).GenericArguments[1];
        if (type == null) return;
        var f = type.GetElementType().Resolve().Fields.First(x => x.Name == s);
        if (f == null) return;
        var field = new FieldReference(s, caller.Module.ImportReference(f.FieldType), type);
        
        lastLdstr.OpCode = MOpCodes.Ldtoken;
        lastLdstr.Operand = caller.Module.ImportReference(field);
        ilp.InsertAfter(lastLdstr, Instruction.Create(MOpCodes.Call, caller.Module.ImportReference(
            GetFieldFromHandle
            )));

        ilp.InsertBefore(il, Instruction.Create(MOpCodes.Ldsflda, GetRefCache(field.FullName, caller.Module)));
        il.OpCode = MOpCodes.Call;
        il.Operand = caller.Module.ImportReference(
            typeof(ReflectionHelperEx).GetMethod("GetFieldRefPointerEx")
            );
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
    public static void Patch_FindFieldInfoEx(MemberReference mr, MethodDefinition caller, Instruction il)
    {
        var lastLdstr = il.Previous;
        if (lastLdstr.OpCode != MOpCodes.Ldstr) return;
        var s = (string)lastLdstr.Operand;

        var type = ((GenericInstanceMethod)mr).GenericArguments[0];
        if (type == null) return;
        var f = type.GetElementType().Resolve().Fields.First(x => x.Name == s);
        if (f == null) return;

        var field = new FieldReference(s, caller.Module.ImportReference(f.FieldType), type);
        
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
