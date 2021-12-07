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
    }
}
