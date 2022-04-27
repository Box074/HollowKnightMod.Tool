
namespace HKTool.Utils;


public static class Helper
{
    private static MethodInfo M_clone = (MethodInfo)FindMethodBase("System.Object::MemberwiseClone")!;
    public static T MemberwiseClone<T>(this T self) => (T)M_clone.FastInvoke(self)!;
    private static object? DeepCloneIn(object? self, int depth, Hashtable table)
    {
        if(self == null) return null;
        
        if(depth <= 0) return self;
        if(table.ContainsKey(self)) return table[self];
        var type = self.GetType();
        if(type == typeof(string)) return self;
        if(type.IsArray)
        {
            var oarr = (Array)self;
            var narr = Array.CreateInstance(type.GetElementType(), oarr.Length);
            table.Add(self, narr);
            for(int i = 0 ; i < oarr.Length ; i++)
            {
                narr.SetValue(DeepCloneIn(oarr.GetValue(i), depth - 1, table), i);;
            }
            return narr;
        }
        var obj = self.MemberwiseClone();
        table.Add(self, obj);
        if(depth == 1) return obj;
        foreach(var v in type.GetFields(HReflectionHelper.Instance))
        {
            v.FastSet(obj, DeepCloneIn(v.FastGet(self), depth - 1, table));
        }

        return obj;
    }
    public static T DeepClone<T>(this T self, int depth = int.MaxValue)
    {
        return (T)DeepCloneIn(self!, depth, new())!;
    }
    public static void CloneFieldTo(this object self, object dest)
    {
        if(self == null || dest == null) return;
        var selfType = self.GetType();
        var destType = dest.GetType();
        var isSubclass = destType.IsSubclassOf(selfType);
        foreach(var v in selfType.GetFields(HReflectionHelper.Instance))
        {
            var val = v.FastGet(self);
            var vt = val?.GetType() ?? v.FieldType;
            if(isSubclass)
            {
                v.FastSet(dest, val);
            }
            else
            {
                var df = destType.GetField(v.Name, HReflectionHelper.Instance);
                if(df != null)
                {
                    if(df.FieldType.IsAssignableFrom(vt)) df.FastSet(dest, val);
                    else if(df.FieldType.IsEnum && vt.IsEnum && val != null)
                    {
                        df.FastSet(dest, Enum.Parse(df.FieldType, val.ToString()));
                    }
                }
            }
        }
    }
}
