
namespace HKTool.Utils;


public static class Helper
{
    public delegate void WithDelegate<T>(ref T self);
    public static T MemberwiseClone<T>(this T self) => (T)System.Instance_Object.MemberwiseClone((object)self!);
    public static T With<T>(this T self, WithDelegate<T> cb)
    {
        cb(ref self);
        return self;
    }
    public static T With<T>(this T self, Action<T> cb)
    {
        cb(self);
        return self;
    }

    public static TValue TryGetOrAddValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> init)
    {
        if(!dict.TryGetValue(key, out var result))
        {
            result = init();
            dict.Add(key, result);
        }
        return result;
    }

    public static T[] Slice<T>(this T[] array, Range range)
    {
        if (array == null)
        {
            throw new ArgumentOutOfRangeException("array");
        }
        (int Offset, int Length) = range.GetOffsetAndLength(array.Length);
        T[] array2;
        if (typeof(T).IsValueType || typeof(T[]) == array.GetType())
        {
            if (Length == 0)
            {
                return Array.Empty<T>();
            }
            array2 = new T[Length];
        }
        else
        {
            array2 = UnsafeUtils.UnsafeCast<Array, T[]>(Array.CreateInstance(array.GetType().GetElementType(), Length));
        }
        StaticArray.FastCopy(array, Offset, array2, 0, Length);
        return array2;
    }
}
