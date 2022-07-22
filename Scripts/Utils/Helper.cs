
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
}
