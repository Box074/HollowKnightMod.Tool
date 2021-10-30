using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace HKTool.Core
{
    public class ParseFakeLibrary
    {
        public static readonly bool IgnoreFake = false;
        public static TypeDefinition cache = null;
        public static ModuleDefinition module = null;
        public static ILProcessor cctorIL = null;
        public static VariableDefinition tempVar = null;
        public static System.Reflection.FieldInfo EmptyTypes = typeof(Type).GetField("EmptyTypes");
        public static System.Reflection.MethodInfo CreateUnSafeInstance = typeof(System.Runtime.Serialization.FormatterServices)
            .GetMethod("GetUninitializedObject");
        public static System.Reflection.MethodInfo M_MakeGenericType = typeof(Type).GetMethod("MakeGenericType");
        public static System.Reflection.MethodInfo FakeIL_GetRTType = typeof(FakeIL).GetMethod("GetRTType");
        public static System.Reflection.MethodInfo FakeIL_GetNType = typeof(FakeIL).GetMethod("GetNType");
        public static System.Reflection.MethodInfo FakeIL_GetAssembly = typeof(FakeIL).GetMethod("GetAssembly");
        public static System.Reflection.MethodInfo FakeIL_CreateGetter = typeof(FakeIL).GetMethod("CreateGetter");
        public static System.Reflection.MethodInfo IL_Getter_Invoke = typeof(IL_GetInstance).GetMethod("Invoke");
        public static System.Reflection.MethodInfo FakeIL_CreateSetter = typeof(FakeIL).GetMethod("CreateSetter");
        public static System.Reflection.MethodInfo IL_Setter_Invoke = typeof(IL_SetInstance).GetMethod("Invoke");
        public static System.Reflection.MethodInfo FakeIL_CreateCaller = typeof(FakeIL).GetMethod("CreateCaller");
        public static System.Reflection.MethodInfo IL_CallMethod_Invoke = typeof(IL_CallMethod).GetMethod("Invoke");
        public static System.Reflection.MethodInfo FakeIL_CreateCast = typeof(FakeIL).GetMethod("CreateCast");
        public static System.Reflection.MethodInfo IL_Cast_Invoke = typeof(IL_Castclass).GetMethod("Invoke");
        public static void CreateTypeCache()
        {
            var t = new TypeDefinition("", "<FakePublic>", TypeAttributes.Class);
            t.BaseType = module.ImportReference(typeof(object));

            var cctor = new MethodDefinition(".cctor", MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                MethodAttributes.RTSpecialName | MethodAttributes.Static,
                module.ImportReference(typeof(void)));
            t.Methods.Add(cctor);
            cctor.Body = new MethodBody(cctor);
            cctorIL = cctor.Body.GetILProcessor();
            tempVar = new VariableDefinition(module.ImportReference(typeof(object)));
            cctor.Body.Variables.Add(tempVar);
            module.Types.Add(t);
            cache = t;
        }
        public static FieldDefinition GetAssemblyRef(AssemblyNameReference n)
        {
            string name = "<Assembly>" + n.FullName;
            var f = cache.Fields.FirstOrDefault(x => x.Name == name);
            if (f == null)
            {
                f = new FieldDefinition(name, FieldAttributes.Static | FieldAttributes.Private,
                    module.ImportReference(typeof(System.Reflection.Assembly)));
                cache.Fields.Add(f);
                cctorIL.Emit(OpCodes.Ldstr, n.Name.StartsWith("fake_") ? n.FullName.Remove(0, 5) : n.FullName);
                cctorIL.Emit(OpCodes.Call, module.ImportReference(FakeIL_GetAssembly));
                cctorIL.Emit(OpCodes.Stsfld, f);
            }
            return f;
        }
        public static FieldDefinition GetTypeRef(AssemblyNameReference ass,string fullName, string Name,TypeReference bt = null,
            params TypeReference[] gt)
        {
            fullName = fullName.Replace("fake_", "");
            Name = Name.Replace("fake_", "");
            string name = "<TypeGetter>"+ ass + "-" + fullName.Replace('.','_');
            var f = cache.Fields.FirstOrDefault(x => x.Name == name);
            if (f == null)
            {
                f = new FieldDefinition(name, FieldAttributes.Static | FieldAttributes.Public,
                    module.ImportReference(typeof(Type)));
                if (fullName.Contains('/'))
                {
                    string pn = fullName.Substring(0, fullName.LastIndexOf('/'));
                    var p = GetTypeRef(null, pn, pn.Split('.', '/').Last());
                    cctorIL.Emit(OpCodes.Ldsfld, p);
                    if (bt == null)
                    {
                        cctorIL.Emit(OpCodes.Ldstr, Name);
                    }
                    else
                    {
                        cctorIL.Emit(OpCodes.Ldstr, GetOrigName(bt.Name));
                    }
                    cctorIL.Emit(OpCodes.Call, module.ImportReference(FakeIL_GetNType));
                }
                else
                {
                    if(ass != null)
                    {
                        cctorIL.Emit(OpCodes.Ldsfld, GetAssemblyRef(ass));
                    }
                    else
                    {
                        cctorIL.Emit(OpCodes.Ldnull);
                    }
                    if (bt == null)
                    {
                        cctorIL.Emit(OpCodes.Ldstr, fullName);
                    }
                    else
                    {
                        cctorIL.Emit(OpCodes.Ldstr, bt.FullName.Replace("fake_",""));
                    }
                    cctorIL.Emit(OpCodes.Call, module.ImportReference(FakeIL_GetRTType));
                    
                }
                /*if(gt.Length > 0)
                {
                    
                    cctorIL.Emit(OpCodes.Ldc_I4, gt.Length);
                    cctorIL.Emit(OpCodes.Newarr, module.ImportReference(typeof(Type)));
                    for(int i = 0; i < gt.Length; i++)
                    {
                        cctorIL.Emit(OpCodes.Ldc_I4, i);
                        cctorIL.Emit(OpCodes.Ldsfld, GetTypeRef(gt[i]));
                        cctorIL.Emit(OpCodes.Stelem_Any, module.ImportReference(typeof(Type)));
                    }
                    cctorIL.Emit(OpCodes.Callvirt, module.ImportReference(M_MakeGenericType));
                }*/
                cctorIL.Emit(OpCodes.Stsfld, f);
                cache.Fields.Add(f);
            }
            return f;
        }
        public static FieldDefinition GetTypeRef(TypeReference type)
        {
            AssemblyNameReference a = type.Scope as AssemblyNameReference;
            if (type.IsGenericInstance)
            {
                throw new NotSupportedException();
                /*TypeReference bt = type.GetElementType();
                return GetTypeRef(a, type.FullName, type.Name, bt, type.GenericParameters.ToArray());*/
            }
            else
            {
                return GetTypeRef(a, type.FullName, type.Name);
            }
        }
        public static MethodDefinition GetFieldGetter(FieldReference f)
        {
            string n = f.DeclaringType.FullName.Replace('.', '_') + "_$" + f.Name;
            string mn = "<FieldGetter>" + n;
            string fn = "<FieldGetterD>" + n;
            var m = cache.Methods.FirstOrDefault(x => x.Name == mn);
            if (m == null)
            {
                var f2 = cache.Fields.FirstOrDefault(x => x.Name == fn);
                if (f2 == null)
                {
                    f2 = new FieldDefinition(fn, FieldAttributes.Static | FieldAttributes.Private,
                        module.ImportReference(typeof(IL_GetInstance)));
                    cache.Fields.Add(f2);

                    cctorIL.Emit(OpCodes.Ldsfld, GetTypeRef(f.DeclaringType));
                    cctorIL.Emit(OpCodes.Ldstr, GetOrigName(f.Name));
                    cctorIL.Emit(OpCodes.Call, module.ImportReference(FakeIL_CreateGetter));
                    cctorIL.Emit(OpCodes.Stsfld, f2);
                }
                m = new MethodDefinition(mn, MethodAttributes.Static | MethodAttributes.Public, module.ImportReference(typeof(object)));
                m.Parameters.Add(new ParameterDefinition(module.ImportReference(typeof(object))));
                var b = new MethodBody(m);
                m.Body = b;
                var il = b.GetILProcessor();
                il.Emit(OpCodes.Ldsfld, f2);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Tail);
                il.Emit(OpCodes.Callvirt, module.ImportReference(IL_Getter_Invoke));
                il.Emit(OpCodes.Ret);
                cache.Methods.Add(m);
            }
            return m;
        }

        public static MethodDefinition GetFieldSetter(FieldReference f)
        {
            string n = f.DeclaringType.FullName.Replace('.', '_') + "_$" + f.Name;
            string mn = "<FieldSetter>" + n;
            string fn = "<FieldSetterD>" + n;
            var m = cache.Methods.FirstOrDefault(x => x.Name == mn);
            if (m == null)
            {
                var f2 = cache.Fields.FirstOrDefault(x => x.Name == fn);
                if (f2 == null)
                {
                    f2 = new FieldDefinition(fn, FieldAttributes.Static | FieldAttributes.Private,
                        module.ImportReference(typeof(IL_SetInstance)));
                    cache.Fields.Add(f2);

                    cctorIL.Emit(OpCodes.Ldsfld, GetTypeRef(f.DeclaringType));
                    cctorIL.Emit(OpCodes.Ldstr, GetOrigName(f.Name));
                    cctorIL.Emit(OpCodes.Call, module.ImportReference(FakeIL_CreateSetter));
                    cctorIL.Emit(OpCodes.Stsfld, f2);
                }
                m = new MethodDefinition(mn, MethodAttributes.Static | MethodAttributes.Public, module.ImportReference(typeof(void)));
                m.Parameters.Add(new ParameterDefinition(module.ImportReference(typeof(object))));
                m.Parameters.Add(new ParameterDefinition(module.ImportReference(typeof(object))));
                var b = new MethodBody(m);
                m.Body = b;
                var il = b.GetILProcessor();
                il.Emit(OpCodes.Ldsfld, f2);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Tail);
                il.Emit(OpCodes.Callvirt, module.ImportReference(IL_Setter_Invoke));
                il.Emit(OpCodes.Ret);
                cache.Methods.Add(m);
            }
            return m;
        }
        public static MethodDefinition GetCast(TypeReference t, bool isUnBox = false)
        {
            string n = t.FullName + "_"+isUnBox.ToString();
            string fn = "<CastF>" + n;
            string mn = "<CastM>" + n;
            var m = cache.Methods.FirstOrDefault(x => x.Name == mn);
            if (m == null)
            {
                var f = cache.Fields.FirstOrDefault(x => x.Name == fn);
                if (f == null)
                {
                    f = new FieldDefinition(fn, FieldAttributes.Static | FieldAttributes.Public, module.ImportReference(typeof(IL_Castclass)));
                    cache.Fields.Add(f);

                    cctorIL.Emit(OpCodes.Ldsfld, GetTypeRef(t));
                    cctorIL.Emit(OpCodes.Ldc_I4, isUnBox ? 1 : 0);
                    cctorIL.Emit(OpCodes.Call, module.ImportReference(FakeIL_CreateCast));
                    cctorIL.Emit(OpCodes.Stsfld, f);
                }
                m = new MethodDefinition(mn, MethodAttributes.Static | MethodAttributes.Public, module.ImportReference(typeof(object)));
                m.Parameters.Add(new ParameterDefinition(module.ImportReference(typeof(object))));
                cache.Methods.Add(m);
                var body = new MethodBody(m);
                m.Body = body;
                var il = body.GetILProcessor();

                il.Emit(OpCodes.Ldsfld, f);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Tail);
                il.Emit(OpCodes.Call, module.ImportReference(IL_Cast_Invoke));
                il.Emit(OpCodes.Ret);
            }
            return m;
        }
        public static MethodDefinition GetMethodCallerTF(MethodDefinition caller)
        {
            if (caller.Parameters.Count == 0) return null;
            string mn = "<MethodCallerTF>" + caller.FullName;
            var m = cache.Methods.FirstOrDefault(x => x.Name == mn);
            if (m == null)
            {
                m = new MethodDefinition(mn, MethodAttributes.Static | MethodAttributes.Public, caller.ReturnType);
                cache.Methods.Add(m);
                var body = new MethodBody(m);
                m.Body = body;
                var il = body.GetILProcessor();
                int pc = caller.Parameters.Count;
                
                for (int i = 0; i < pc; i++)
                {
                    m.Parameters.Add(new ParameterDefinition(module.ImportReference(typeof(object))));
                }
                il.Emit(OpCodes.Ldarg, m.Parameters.Last());
                for(int i = 0; i < pc - 1; i++)
                {
                    il.Emit(OpCodes.Ldarg, m.Parameters[i]);
                }
                il.Emit(OpCodes.Tail);
                il.Emit(OpCodes.Call, caller);
                il.Emit(OpCodes.Ret);
            }
            return m;

        }
        public static MethodDefinition GetMethodCaller(MethodReference mr, int cv)
        {
            string n = mr.ToString() + "_" + cv;
            string mn = "<MethodCaller>" + n;
            string fn = "<MethodCallerD>" + n;
            var m = cache.Methods.FirstOrDefault(x => x.Name == mn);
            if (m == null)
            {
                int pc = mr.Parameters.Count;
                var f = cache.Fields.FirstOrDefault(x => x.Name == fn);
                if(f == null)
                {
                    f = new FieldDefinition(fn, FieldAttributes.Static | FieldAttributes.Private,
                        module.ImportReference(typeof(IL_CallMethod)));
                    cache.Fields.Add(f);


                    cctorIL.Emit(OpCodes.Ldsfld, GetTypeRef(mr.DeclaringType));
                    cctorIL.Emit(OpCodes.Ldstr, GetOrigName(mr.Name));
                    cctorIL.Emit(OpCodes.Ldc_I4, cv);
                    
                    if (pc > 0)
                    {
                        cctorIL.Emit(OpCodes.Ldc_I4, pc);
                        cctorIL.Emit(OpCodes.Newarr, module.ImportReference(typeof(Type)));
                        cctorIL.Emit(OpCodes.Stloc, tempVar);

                        for (int i = 0; i < pc; i++)
                        {
                            cctorIL.Emit(OpCodes.Ldloc, tempVar);
                            cctorIL.Emit(OpCodes.Ldc_I4, i);
                            cctorIL.Emit(OpCodes.Ldsfld, GetTypeRef(mr.Parameters[i].ParameterType));
                            //cctorIL.Emit(OpCodes.Ldnull);
                            cctorIL.Emit(OpCodes.Stelem_Ref);
                        }
                        cctorIL.Emit(OpCodes.Ldloc, tempVar);
                    }
                    else
                    {
                        cctorIL.Emit(OpCodes.Ldsfld, module.ImportReference(EmptyTypes));
                    }
                    cctorIL.Emit(OpCodes.Call, module.ImportReference(FakeIL_CreateCaller));
                    cctorIL.Emit(OpCodes.Stsfld, f);

                }
                m = new MethodDefinition(mn, MethodAttributes.Public | MethodAttributes.Static, 
                    mr.ReturnType.FullName == "System.Void" ? mr.ReturnType : module.ImportReference(typeof(object))
                    );
                if (mr.HasThis)
                {
                    m.Parameters.Add(new ParameterDefinition(module.ImportReference(typeof(object))));
                }
                for(int i = 0; i < pc; i++)
                {
                    m.Parameters.Add(new ParameterDefinition(module.ImportReference(typeof(object))));
                }
                cache.Methods.Add(m);
                var body = new MethodBody(m);
                m.Body = body;
                var il = body.GetILProcessor();
                var va = new VariableDefinition(module.ImportReference(typeof(object[])));
                body.Variables.Add(va);

                il.Emit(OpCodes.Ldsfld, f);
                if (!mr.HasThis)
                {
                    il.Emit(OpCodes.Ldnull);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                }
                

                il.Emit(OpCodes.Ldc_I4, pc);
                il.Emit(OpCodes.Newarr, module.ImportReference(typeof(object)));
                il.Emit(OpCodes.Stloc, va);
                for(int i = 0; i < pc; i++)
                {
                    il.Emit(OpCodes.Ldloc, va);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg, mr.HasThis ? (i + 1) : i);
                    //il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Stelem_Any, module.ImportReference(typeof(object)));
                }
                il.Emit(OpCodes.Ldloc, va);
                if (mr.ReturnType.FullName != "System.Void")
                {
                    il.Emit(OpCodes.Tail);
                }
                il.Emit(OpCodes.Callvirt, module.ImportReference(IL_CallMethod_Invoke));
                if (mr.ReturnType.FullName == "System.Void")
                {
                    il.Emit(OpCodes.Pop);
                }
                il.Emit(OpCodes.Ret);
            }
            return m;
        }
        
        public static bool ShouldConvert(TypeReference t)
        {
            if (IgnoreFake) return true;
            if (t.Scope is AssemblyNameReference a)
            {
                if (a.Name.StartsWith("fake_"))
                {
                    if (t.Name.StartsWith("fake_")) return true;
                }
            }
            return false;
        }
        public static string GetOrigName(string n)
        {
            if (n.StartsWith("fake_")) return n.Remove(0, 5);
            return n;
        }
        public static bool ShouldConvert(MemberReference mr)
        {
            if(IgnoreFake) return true;
            TypeReference t = mr.DeclaringType;
            if (t.Scope is AssemblyNameReference a)
            {
                if (a.Name.StartsWith("fake_"))
                {
                    if (t.Name.StartsWith("fake_") || mr.Name.StartsWith("fake_")) return true;
                }
            }
            return false;
        }
        public static void ParseMethod(MethodDefinition m)
        {

            if (!m.HasBody) return;

            var body = m.Body;
            foreach (var v2 in body.Variables)
            {
                if (ShouldConvert(v2.VariableType))
                {
                    v2.VariableType = module.ImportReference(typeof(object));
                }
            }
            //if (!m.HasBody) return;
            if (m.Body.Instructions.Count == 0) return;
            ILProcessor il = m.Body.GetILProcessor();
            Instruction i = m.Body.Instructions[0];
            VariableDefinition v = new VariableDefinition(module.ImportReference(typeof(object)));
            body.Variables.Add(v);


            while (i != null)
            {
                if (i.OpCode == OpCodes.Ldsfld)
                {
                    FieldReference fr = (FieldReference)i.Operand;
                    if (ShouldConvert(fr))
                    {
                        i.OpCode = OpCodes.Call;
                        i.Operand = GetFieldGetter(fr);
                        il.InsertBefore(i, Instruction.Create(OpCodes.Ldnull));
                    }
                }
                else if (i.OpCode == OpCodes.Stsfld)
                {
                    FieldReference fr = (FieldReference)i.Operand;
                    if (ShouldConvert(fr))
                    {
                        il.InsertBefore(i, Instruction.Create(OpCodes.Stloc, v));
                        il.InsertBefore(i, Instruction.Create(OpCodes.Ldnull));
                        il.InsertBefore(i, Instruction.Create(OpCodes.Ldloc, v));

                        i.OpCode = OpCodes.Call;
                        i.Operand = GetFieldSetter(fr);
                    }
                }
                else if (i.OpCode == OpCodes.Ldfld)
                {
                    FieldReference fr = (FieldReference)i.Operand;
                    if (ShouldConvert(fr))
                    {
                        i.OpCode = OpCodes.Call;
                        i.Operand = GetFieldGetter(fr);
                    }
                }
                else if (i.OpCode == OpCodes.Stfld)
                {
                    FieldReference fr = (FieldReference)i.Operand;
                    if (ShouldConvert(fr))
                    {
                        i.OpCode = OpCodes.Call;
                        i.Operand = GetFieldSetter(fr);
                    }
                }
                else if (i.OpCode == OpCodes.Call)
                {
                    MethodReference mr = (MethodReference)i.Operand;
                    if (ShouldConvert(mr))
                    {
                        i.OpCode = OpCodes.Call;
                        i.Operand = GetMethodCaller(mr, 0);
                    }
                }
                else if (i.OpCode == OpCodes.Callvirt)
                {
                    MethodReference mr = (MethodReference)i.Operand;
                    if (ShouldConvert(mr))
                    {
                        i.OpCode = OpCodes.Call;
                        i.Operand = GetMethodCaller(mr, 1);
                    }
                }
                else if (i.OpCode == OpCodes.Newobj)
                {
                    MethodReference mr = (MethodReference)i.Operand;
                    if (ShouldConvert(mr))
                    {
                        i.OpCode = OpCodes.Call;
                        i.Operand = GetMethodCallerTF(GetMethodCaller(mr, 1));


                        il.InsertBefore(i, Instruction.Create(OpCodes.Ldsfld, GetTypeRef(mr.DeclaringType)));
                        il.InsertBefore(i, Instruction.Create(OpCodes.Call, module.ImportReference(CreateUnSafeInstance)));
                        il.InsertBefore(i, Instruction.Create(OpCodes.Stloc, v));
                        il.InsertBefore(i, Instruction.Create(OpCodes.Ldloc, v));
                        il.InsertAfter(i, Instruction.Create(OpCodes.Ldloc, v));
                    }
                }
                else if (i.OpCode == OpCodes.Castclass)
                {
                    TypeReference tr = (TypeReference)i.Operand;
                    if (ShouldConvert(tr))
                    {
                        i.OpCode = OpCodes.Call;
                        i.Operand = GetCast(tr, false);
                    }
                }
                else if (i.OpCode == OpCodes.Unbox)
                {
                    TypeReference tr = (TypeReference)i.Operand;
                    if (ShouldConvert(tr))
                    {
                        i.OpCode = OpCodes.Call;
                        i.Operand = GetCast(tr, true);
                    }
                }
                else if (i.OpCode == OpCodes.Unbox_Any)
                {
                    TypeReference tr = (TypeReference)i.Operand;
                    if (ShouldConvert(tr))
                    {
                        i.OpCode = OpCodes.Call;
                        i.Operand = GetCast(tr, true);
                    }
                }
                i = i.Next;
            }

        }

        public static void ParseType(TypeDefinition t)
        {
            if (t == cache) return;
            foreach(var v in t.Methods)
            {
                foreach(var p in v.Parameters)
                {
                    if (p.ParameterType.FullName == "System.Void") continue;
                    if (ShouldConvert(p.ParameterType))
                    {
                        //p.ParameterType = module.ImportReference(typeof(object));
                    }
                }
                if (v.ReturnType.FullName != "System.Void" && ShouldConvert(v.ReturnType))
                {
                    v.ReturnType = module.ImportReference(typeof(object));
                }
                ParseMethod(v);
            }
            foreach(var v in t.Fields)
            {
                if (v.FieldType.FullName == "System.Void") continue;
                if (ShouldConvert(v.FieldType))
                {
                    v.FieldType = module.ImportReference(typeof(object));
                }
            }
            foreach(var v in t.NestedTypes)
            {
                ParseType(v);
            }
        }
        public static void ParseModule(ModuleDefinition m)
        {
            module = m;
            CreateTypeCache();
            foreach(var v in m.Types)
            {
                ParseType(v);
            }
            int c = 0;
            foreach(var v in cache.Fields)
            {
                v.Name = "F" + c++;
            }
            foreach(var v in cache.Methods)
            {
                if (v.IsConstructor) continue;
                v.Name = "M" + c++;
            }
            cctorIL.Emit(OpCodes.Ret);
            foreach(var v in m.AssemblyReferences)
            {
                if (v.Name.StartsWith("fake_"))
                {
                    v.Name = v.Name.Remove(0, 5);
                }
            }
        }
        public static AssemblyDefinition ParseAssembly(AssemblyDefinition src)
        {
            var s = new MemoryStream();
            src.Write(s);
            s.Position = 0;
            AssemblyDefinition ad = AssemblyDefinition.ReadAssembly(s);
            ad.Name.PublicKey = null;
            foreach (var v in ad.Modules) ParseModule(v);
            return ad;
        }
        public static void ParseAssembly(string src, string dst)
        {
            ParseAssembly(AssemblyDefinition.ReadAssembly(src)).Write(dst);
        }
    }
}
