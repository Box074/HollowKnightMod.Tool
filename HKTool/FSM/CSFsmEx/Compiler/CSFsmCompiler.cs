using Modding.Utils;
using MonoMod.Utils.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.FSM.CSFsmEx.Compiler
{
    internal partial class CSFsmCompiler
    {
        private Type m_type = null!;
        private CecilILGenerator il = null!;
        private Dictionary<Type, Stack<LocalBuilder>> m_tempLocal = new();
        
        public static readonly MethodInfo info_mergeFsmVar = Info.OfMethod<CSFsmBuildContext>(nameof(CSFsmBuildContext.MergeFsmVar));
        public static readonly MethodInfo info_findChild = Info.OfMethod<CSFsmBuildContext>(nameof(CSFsmBuildContext.FindChild));
        
        public static readonly MethodInfo info_ctx_getFSM = Info.OfPropertyGet<CSFsmBuildContext>(nameof(CSFsmBuildContext.Fsm));
        public static readonly MethodInfo info_ctx_getHost = Info.OfPropertyGet<CSFsmBuildContext>(nameof(CSFsmBuildContext.Host));
        public static readonly MethodInfo info_ctx_getGameObject = Info.OfPropertyGet<CSFsmBuildContext>(nameof(CSFsmBuildContext.GameObject));

        public static readonly MethodInfo info_fsm_getvars = Info.OfPropertyGet<Fsm>(nameof(Fsm.Variables));
        public static readonly MethodInfo info_fsmvar_getvar = Info.OfMethod<FsmVariables>(nameof(FsmVariables.GetVariable));

        public readonly MethodInfo info_get_or_add_component = Info.OfMethod("Assembly-CSharp", "Modding.Utils.UnityExtensions", 
            nameof(UnityExtensions.GetOrAddComponent));

        private LocalBuilder AllocTempLocal(Type type)
        {
            var list = m_tempLocal.TryGetOrAddValue(type, () => new());
            if (list.TryPop(out var result)) return result;
            return il.DeclareLocal(type);
        }
        private void RecycleTempLocal(LocalBuilder local)
        {
            var list = m_tempLocal.TryGetOrAddValue(local.LocalType, () => new());
            list.Push(local);
        }

        public void Compile(Type type, MethodDefinition method)
        {
            m_type = type;

            var ilp = method.Body.GetILProcessor();
            var cig = il = new CecilILGenerator(ilp);

            var local_ctx = cig.DeclareLocal(typeof(CSFsmBuildContext));
            var local_host = cig.DeclareLocal(type);

            cig.Emit(OpCodes.Ldarg_0);
            cig.Emit(OpCodes.Dup);
            cig.Emit(OpCodes.Stloc, local_ctx);
            cig.Emit(OpCodes.Call, info_ctx_getHost);
            cig.Emit(OpCodes.Stloc, local_host);

            EmitFsmBindings(local_host, local_ctx);
            EmitFsmVarModifier(local_host, local_ctx);
            EmitFsmStateBuilder(local_host, local_ctx);

            cig.Emit(OpCodes.Ret);
        }

        private void EmitFsmBindings(LocalBuilder host, LocalBuilder ctx)
        {
            var dict_bindings = new Dictionary<string, List<FieldInfo>>();

            foreach (var f in m_type.GetFields(HReflectionHelper.Instance))
            {
                var attr = f.GetCustomAttribute<CSFsmHost.BindingAttribute>();
                if (attr == null) continue;
                if (!f.FieldType.IsSubclassOf(typeof(UObject))) throw new NotSupportedException();
                dict_bindings.TryGetOrAddValue(attr.childPath, () => new()).Add(f);
            }

            if (dict_bindings.Count == 0) return;

            var l_go = il.DeclareLocal(typeof(GameObject));

            foreach((var path, var fields) in dict_bindings)
            {
                if(string.IsNullOrEmpty(path))
                {
                    il.Emit(OpCodes.Ldloc, ctx);
                    il.Emit(OpCodes.Call, info_ctx_getGameObject);
                }
                else
                {
                    var pparts = path.Split('/');
                    il.Emit(OpCodes.Ldloc, ctx);
                    il.Emit(OpCodes.Ldc_I4, pparts.Length);
                    il.Emit(OpCodes.Newarr, typeof(string));

                    for(int i = 0; i < pparts.Length; i++)
                    {
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Ldc_I4, i);
                        il.Emit(OpCodes.Ldstr, pparts[i]);
                        il.Emit(OpCodes.Stelem, typeof(string));
                    }

                    il.Emit(OpCodes.Call, info_findChild);
                }

                var emptyLabel = il.DefineLabel();
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc, l_go);
                il.Emit(OpCodes.Brfalse, emptyLabel);

                foreach(var field in fields)
                {
                    il.Emit(OpCodes.Ldloc, host);
                    il.Emit(OpCodes.Ldloc, l_go);
                    var t = field.FieldType;
                    if(t != typeof(GameObject))
                    {
                        var gmi = info_get_or_add_component.MakeGenericMethod(t);
                        il.Emit(OpCodes.Call, gmi);
                    }
                    il.Emit(OpCodes.Stfld, field);
                }

                il.Emit(OpCodes.Nop);
                il.MarkLabel(emptyLabel);
                il.Emit(OpCodes.Nop);
            }
        }

        private void EmitFsmVarModifier(LocalBuilder host, LocalBuilder ctx)
        {
            
            var dict_fsmVars = new Dictionary<Type, List<FieldInfo>>();

            foreach (var f in m_type.GetFields(HReflectionHelper.Instance))
            {
                if(!f.IsDefined(typeof(CSFsmHost.FsmVarAttribute), false)) continue;
                if (!f.FieldType.IsSubclassOf(typeof(NamedVariable))) throw new NotSupportedException();
                dict_fsmVars.TryGetOrAddValue(f.FieldType, () => new()).Add(f);
            }

            if (dict_fsmVars.Count == 0) return;

            var l_fsmVar = il.DeclareLocal(typeof(FsmVariables));
            var l_index = il.DeclareLocal(typeof(int));
            
            il.Emit(OpCodes.Ldloc, ctx);
            il.Emit(OpCodes.Call, info_ctx_getFSM);
            il.Emit(OpCodes.Call, info_fsm_getvars);
            il.Emit(OpCodes.Stloc, l_fsmVar);

            foreach ((var type, var fields) in dict_fsmVars)
            {
                if(fields.Count == 0) continue;
                var prop = typeof(FsmVariables).GetProperties().First(x =>
                    x.Name.EndsWith("Variables") &&
                    x.PropertyType.GetElementType() == type
                    );
                var ctor = type.GetConstructor(new Type[] { typeof(string) });

                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stloc, l_index);

                il.Emit(OpCodes.Ldloc, l_fsmVar);
                il.Emit(OpCodes.Ldc_I4, fields.Count);
                il.Emit(OpCodes.Newarr, type); //S: Vars, Array
 
                foreach(var f in fields)
                {
                    var varAttr = f.GetCustomAttribute<CSFsmHost.FsmVarAttribute>();
                    var varName = string.IsNullOrEmpty(varAttr.varName) ? f.Name : varAttr.varName;

                    il.Emit(OpCodes.Dup); //Stack: Array

                    il.Emit(OpCodes.Ldloc, host);
                    il.Emit(OpCodes.Ldloc, l_fsmVar);
                    il.Emit(OpCodes.Ldstr, varName);
                    il.Emit(OpCodes.Call, info_fsmvar_getvar);
                    il.Emit(OpCodes.Castclass, type); //Stack: Array, FsmVar

                    var tl = AllocTempLocal(f.FieldType);
                    il.Emit(OpCodes.Stloc, tl); //Stack: Array
                    il.Emit(OpCodes.Ldloc, host); //Stack: Array, Host
                    il.Emit(OpCodes.Ldloc, tl); //Stack: Array, Host, FsmVar
                    il.Emit(OpCodes.Stfld, f); //Stack: Array

                    il.Emit(OpCodes.Ldloc, tl); //Stack: Array, FsmVar

                    RecycleTempLocal(tl);

                    var label_nonull = il.DefineLabel();

                    il.Emit(OpCodes.Brtrue, label_nonull); //Stack: Array

                    il.Emit(OpCodes.Ldloc, l_index);
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Add);
                    il.Emit(OpCodes.Stloc, l_index); //Stack: Array, Index

                    il.Emit(OpCodes.Ldloc, host); //Stack: Array, Index, Host

                    il.Emit(OpCodes.Ldstr, varName);
                    il.Emit(OpCodes.Newobj, ctor); //Stack: Array, Index, Host, FsmVar
                    
                    il.Emit(OpCodes.Stfld, f); //Stack: Array, Index
                    il.Emit(OpCodes.Ldloc, host);
                    il.Emit(OpCodes.Ldfld, f); //Stack: Array, Index, FsmVar
 
                    il.Emit(OpCodes.Stelem, type); //Stack: 
                    il.Emit(OpCodes.Ldnull); //Stack: null
                    il.Emit(OpCodes.Nop);

                    il.MarkLabel(label_nonull); //Stack: Array | null
                    il.Emit(OpCodes.Pop); //Stack: 
                    il.Emit(OpCodes.Nop);

                }
                il.Emit(OpCodes.Ldloc, l_fsmVar); //S: Vars, Array, Vars
                il.Emit(OpCodes.Call, prop.GetMethod); //S: Vars, Array, Array
                il.Emit(OpCodes.Ldloc, l_index); //S: Vars, Array, Array, Int
                il.Emit(OpCodes.Call, info_mergeFsmVar.MakeGenericMethod(type)); //S: Array
                il.Emit(OpCodes.Call, prop.SetMethod);  //S:
            }
        }
    }
}
