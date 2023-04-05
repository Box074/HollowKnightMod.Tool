using MonoMod.Utils.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HKTool.FSM.CSFsmEx.CSFsmHost;

namespace HKTool.FSM.CSFsmEx.Compiler
{
    internal partial class CSFsmCompiler
    {
        public static readonly MethodInfo info_initState = Info.OfMethod<CSFsmBuildContext>(nameof(CSFsmBuildContext.InitState));
        public static readonly MethodInfo info_getActionWalker = Info.OfMethod<CSFsmBuildContext>(nameof(CSFsmBuildContext.GetActionWalker));
        public static readonly MethodInfo info_getNextAction = Info.OfMethod<CSFsmBuildContext>(nameof(CSFsmBuildContext.GetNextAction));
        public static readonly MethodInfo info_getNextActionOrNew = Info.OfMethod<CSFsmBuildContext>(nameof(CSFsmBuildContext.GetNextActionOrNew));
        public static readonly MethodInfo info_getAllActions = Info.OfMethod<CSFsmBuildContext>(nameof(CSFsmBuildContext.GetAllActions));
        public static readonly MethodInfo info_registerFsmTransition = Info.OfMethod<CSFsmBuildContext>(nameof(CSFsmBuildContext.RegisterFsmTransition));

        public readonly MethodInfo info_fsmevent_get = Info.OfMethod<FsmEvent>(nameof(FsmEvent.GetFsmEvent));
        private void EmitFsmStateBuilder(LocalBuilder host, LocalBuilder ctx)
        {
            foreach(var m in m_type.GetMethods(HReflectionHelper.Instance))
            {
                var attr = m.GetCustomAttribute<CSFsmHost.FsmStateAttribute>();
                if (attr == null) continue;
                
                var stateName = string.IsNullOrEmpty(attr.name) ? m.Name : attr.name;

                il.Emit(OpCodes.Ldloc, ctx);
                il.Emit(OpCodes.Ldstr, stateName);
                il.Emit(OpCodes.Call, info_initState);

                var dict_actionWalkers = new Dictionary<Type, LocalBuilder>();
                var list_allActionTypes = new List<Type>();
                foreach(var p in m.GetParameters())
                {
                    if(p.ParameterType.IsSubclassOf(typeof(FsmStateAction)))
                    {
                        list_allActionTypes.Add(p.ParameterType);
                        continue;
                    }

                    if (!IsValueTupe(p.ParameterType)) continue;
                    var types = FlatValueTupe(p.ParameterType);
                    if(types.All(x => x.IsSubclassOf(typeof(FsmStateAction))))
                    {
                        list_allActionTypes.AddRange(types);
                    }
                }
                foreach (var fat in list_allActionTypes)
                {
                    if (!dict_actionWalkers.ContainsKey(fat))
                    {
                        var walker = AllocTempLocal(typeof(CSFsmStateActionWalker<>).MakeGenericType(fat));
                        dict_actionWalkers.Add(fat, walker);
                        il.Emit(OpCodes.Ldloc, ctx);
                        il.Emit(OpCodes.Call, info_getActionWalker.MakeGenericMethod(fat));
                        il.Emit(OpCodes.Stloc, walker);
                    }
                }

                il.Emit(OpCodes.Ldloc, ctx);
                il.Emit(OpCodes.Ldloc, host);
                bool hasInit = false;
                foreach(var p in m.GetParameters())
                {
                    var evAttr = p.GetCustomAttribute<CSFsmHost.FsmTransitionAttribute>();
                    if(evAttr != null)
                    {
                        il.Emit(OpCodes.Ldloc, ctx);
                        var evName = string.IsNullOrEmpty(evAttr.eventName) ? Guid.NewGuid().ToString().ToUpper() : evAttr.eventName;
                        
                        il.Emit(OpCodes.Ldstr, evName);
                        il.Emit(OpCodes.Ldstr, evAttr.targetName);
                        if(p.ParameterType == typeof(FsmEvent))
                        {
                            il.Emit(OpCodes.Call, info_registerFsmTransition);
                        } else
                        {
                            throw new NotSupportedException();
                        }
                        continue;
                    }
                    if(IsValueTupe(p.ParameterType) && FlatValueTupe(p.ParameterType)
                        .All(x => x.IsSubclassOf(typeof(FsmStateAction))))
                    {
                        EmitOrigActionBindTuple(p.ParameterType, ctx, dict_actionWalkers, p.IsDefined(typeof(GetOrAddAttribute)));
                        continue;
                    }
                    if(p.ParameterType.IsArray && 
                        (p.ParameterType.GetElementType().IsSubclassOf(typeof(FsmStateAction)) || 
                        p.ParameterType.GetElementType() == typeof(FsmStateAction)))
                    {
                        il.Emit(OpCodes.Ldloc, ctx);
                        il.Emit(OpCodes.Call, info_getAllActions.MakeGenericMethod(p.ParameterType.GetElementType()));
                        continue;
                    }
                    if(p.ParameterType.IsSubclassOf(typeof(FsmStateAction)))
                    {
                        EmitOrigActionBind(dict_actionWalkers, p.ParameterType, p.IsDefined(typeof(GetOrAddAttribute)));
                        continue;
                    }
                    if(p.ParameterType == typeof(FsmState))
                    {
                        il.Emit(OpCodes.Ldloc, host);
                        il.Emit(OpCodes.Call, Info.OfPropertyGet<CSFsmHost>(nameof(CSFsmHost.CurrentStateMetadata)));
                        il.Emit(OpCodes.Ldfld, Info.OfField<CSFsmStateMetadata>(nameof(CSFsmStateMetadata.state)));
                        continue;
                    }
                    if(p.ParameterType == typeof(CSFsmStateMetadata))
                    {
                        hasInit = true;
                        il.Emit(OpCodes.Ldloc, host);
                        il.Emit(OpCodes.Call, Info.OfPropertyGet<CSFsmHost>(nameof(CSFsmHost.CurrentStateMetadata)));
                        continue;
                    }
                    il.Emit(OpCodes.Initobj, p.ParameterType);
                }
                il.Emit(OpCodes.Call, m);
                il.Emit(OpCodes.Ldc_I4, hasInit ? 1 : 0);
                il.Emit(OpCodes.Call, Info.OfMethod<CSFsmBuildContext>(nameof(CSFsmBuildContext.RegisterState)));
                foreach((_, var lb) in dict_actionWalkers)
                {
                    RecycleTempLocal(lb);
                }
            }
        }
        
        private void EmitOrigActionBindTuple(Type tupleType, LocalBuilder ctx, 
            Dictionary<Type, LocalBuilder> walkers, bool create)
        {
            var ga = tupleType.GenericTypeArguments;
            for(int i = 0; i < ga.Length && i < 7; i++)
            {
                var fat = ga[i];
                EmitOrigActionBind(walkers, fat, create);
            }
            if (ga.Length == 8)
            {
                EmitOrigActionBindTuple(ga[7], ctx, walkers, create);
            }
            il.Emit(OpCodes.Newobj, tupleType.GetConstructors().First());
        }

        private void EmitOrigActionBind(Dictionary<Type, LocalBuilder> walkers, Type type, bool create = false)
        {
            var walker = walkers[type];
            il.Emit(OpCodes.Ldloc, walker);
            il.Emit(OpCodes.Call, (create ? info_getNextActionOrNew : info_getNextAction).MakeGenericMethod(type));
        }

        private bool IsValueTupe(Type type) => type.FullName.StartsWith("System.ValueTuple") && type.IsGenericType;
        private void FlatValueTupe(Type type, List<Type> types)
        {
            var ga = type.GenericTypeArguments;
            if (ga.Length < 8)
            {
                types.AddRange(ga);
                return;
            }
            var reset = ga[7];
            types.AddRange(ga.Take(7));
            FlatValueTupe(reset, types);
        }
        private Type[] FlatValueTupe(Type type)
        {
            List<Type> types = new();
            FlatValueTupe(type, types);
            return types.ToArray();
        }
    }
}
