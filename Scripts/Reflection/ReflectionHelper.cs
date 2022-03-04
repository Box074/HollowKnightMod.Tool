using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace HKTool.Reflection
{
    public static class ReflectionHelper
    {
        public static BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic 
            | BindingFlags.Instance | BindingFlags.Static;
        public static ReflectionObject CreateReflectionObject(this object obj) => new ReflectionObject(obj);
        public static ReflectionObject CreateReflectionObject(this Type type) => new ReflectionObject(type);
        public static Type FindType(string fullname)
        {
            foreach(var v in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = v.GetType(fullname);
                if (t != null) return t;
            }
            return null;
        }
        public static object FastInvoke(this MethodInfo m, object @this, params object[] args) => FastReflection.CallMethod(@this, m, args);
        public static object FastGet(this FieldInfo f, object @this) => FastReflection.GetField(@this, f);
        public static void FastSet(this FieldInfo f, object @this, object val) => FastReflection.SetField(@this, f, val);
        public static object FastGet(this PropertyInfo p, object @this) => p.GetMethod.FastInvoke(@this);
        public static void FastSet(this PropertyInfo p, object @this, object val) => p.SetMethod.FastInvoke(@this, val);
    }
}
