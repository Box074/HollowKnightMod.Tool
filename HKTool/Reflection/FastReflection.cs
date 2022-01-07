using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using MonoMod.Utils;
using HKTool.Reflection.Runtime;

namespace HKTool.Reflection
{
    static class FastReflection
    {
        public static Dictionary<FieldInfo, RD_SetField> fsetter = new Dictionary<FieldInfo, RD_SetField>();
        public static Dictionary<FieldInfo, RD_GetField> fgetter = new Dictionary<FieldInfo, RD_GetField>();
        public static Dictionary<MethodInfo, FastReflectionDelegate> mcaller =
            new Dictionary<MethodInfo, FastReflectionDelegate>();
        public static RD_GetField GetGetter(FieldInfo field)
        {
            if (!fgetter.TryGetValue(field, out var getter))
            {
                DynamicMethod dm = new DynamicMethod("", MethodAttributes.Static | MethodAttributes.Public,
                    CallingConventions.Standard, typeof(object), new Type[]{
                        typeof(object)
                        }, field.DeclaringType, true);
                var il = dm.GetILGenerator();

                if (!field.IsStatic)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, field);
                }
                else
                {
                    il.Emit(OpCodes.Ldsfld, field);
                }
                il.Emit(OpCodes.Ret);
                getter = (RD_GetField)dm.CreateDelegate(typeof(RD_GetField));
            }
            return getter;
        }
        public static RD_SetField GetSetter(FieldInfo field)
        {
            if (!fsetter.TryGetValue(field, out var setter))
            {
                DynamicMethod dm = new DynamicMethod("", MethodAttributes.Static | MethodAttributes.Public,
                    CallingConventions.Standard, typeof(void), new Type[]{
                        typeof(object),
                        typeof(object)
                        }, field.DeclaringType, true);
                var il = dm.GetILGenerator();

                if (field.IsStatic)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Stsfld, field);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Stfld, field);
                }
                il.Emit(OpCodes.Ret);
                setter = (RD_SetField)dm.CreateDelegate(typeof(RD_SetField));
                fsetter[field] = setter;
            }
            return setter;
        }
        internal static object CallMethod(object @this, MethodInfo method, object[] args)
        {
            if (!mcaller.TryGetValue(method, out var caller))
            {
                caller = method.CreateFastDelegate(true);
                mcaller[method] = caller;
            }
            return caller(@this, args);
        }
        internal static object GetField(object @this, FieldInfo field)
        {
            return GetGetter(field)(@this);
        }
        internal static void SetField(object @this, FieldInfo field, object val)
        {
            GetSetter(field)(@this, val);
        }
    }
}
