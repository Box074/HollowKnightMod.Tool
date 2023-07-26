using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.Reflection
{
    internal static class NullableUtils
    {
        public static byte[]? GetNullableFlags(this IEnumerable<CustomAttributeData> attrs)
        {
            var nullable = attrs.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
            if (nullable == null || nullable.ConstructorArguments.Count == 0) return null;
            var ctorVal = nullable.ConstructorArguments[0].Value;
            if(ctorVal is byte[] result_a) return result_a;
            if(ctorVal is byte result_b) return new[] { result_b };
            return null;
        }
    }
}
