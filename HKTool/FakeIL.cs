using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace HKTool
{
    public delegate void IL_SetInstance(object obj, object val);
    public delegate object IL_GetInstance(object obj);
    public delegate object IL_CallMethod(object @this, object[] args);
    public delegate object IL_Castclass(object obj);
    public class FakeIL
    {
        public static Assembly GetAssembly(string name)
        {
            return Assembly.Load(name);
        }
        public static Type GetRTType(Assembly ass, string name)
        {
            if (ass != null) return ass.GetType(name);
            foreach(var v in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t;
                if((t = v.GetType(name))!= null){
                    return t;
                }
            }
            return null;
        }
        public static Type GetNType(Type t, string name)
        {
            return t.GetNestedType(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        }
        public static IL_Castclass CreateCast(Type dst, bool isUnBox)
        {
            var dm = new DynamicMethod("", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(object), new Type[]
            {
                typeof(object)
            }, dst, true);

            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            if (isUnBox && dst.IsValueType) il.Emit(OpCodes.Unbox, dst);
            else  il.Emit(OpCodes.Castclass, dst);
            il.Emit(OpCodes.Ret);
            return (IL_Castclass)dm.CreateDelegate(typeof(IL_Castclass));
        }

        public static IL_GetInstance CreateGetter(Type t,string name)
        {
            var dm = new DynamicMethod("", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(object), new Type[]
            {
                typeof(object)
            }, t, true);
            
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static));
            il.Emit(OpCodes.Ret);
            return (IL_GetInstance)dm.CreateDelegate(typeof(IL_GetInstance));
        }

        public static IL_SetInstance CreateSetter(Type t, string name)
        {
            var dm = new DynamicMethod("", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(object), new Type[]
            {
                typeof(object),
                typeof(object)
            }, t, true);

            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Stfld, t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static));
            il.Emit(OpCodes.Ret);
            return (IL_SetInstance)dm.CreateDelegate(typeof(IL_SetInstance));
        }

        public static IL_CallMethod CreateCaller(Type t, string name,int cv, Type[] args)
        {
            var m = t.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Static | BindingFlags.Instance, Type.DefaultBinder, args, null);
            ConstructorInfo c = null;
            if (m == null)
            {
                c = t.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, args, null);
                if (c == null)
                {
                    throw new MissingMethodException(name);
                }
                
            }
            System.Diagnostics.Trace.WriteLine($"Create Caller: { m?.ToString() ?? c?.ToString() }");
            DynamicMethod dm = new DynamicMethod("", MethodAttributes.Static | MethodAttributes.Public, 
                CallingConventions.Standard, typeof(object), new Type[]
            {
                typeof(object),
                typeof(object[])
            }, t, true);
            var il = dm.GetILGenerator();
            if(m == null || !m.IsStatic)
                il.Emit(OpCodes.Ldarg_0);
            
            for(int i = 0; i < args.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem, typeof(object));
            }
            if (cv == 1 && m != null)
            {
                il.Emit(OpCodes.Callvirt, m);
            }
            else
            {
                if (m != null)
                {
                    il.Emit(OpCodes.Call, m);
                }
                else
                {
                    il.Emit(OpCodes.Call, c);
                }
            }
            if (m == null) il.Emit(OpCodes.Ldnull);
            if (m?.ReturnType == typeof(void)) il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);
            return (IL_CallMethod)dm.CreateDelegate(typeof(IL_CallMethod));
        }
    }
}
