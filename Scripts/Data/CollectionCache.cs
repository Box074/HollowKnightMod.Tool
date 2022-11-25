
namespace HKTool.Data;

public static class CollectionCache<T> where T : class, new()
{
    private static List<WeakReference<T>?> cache = new();
    private static ConditionalWeakTable<T, Holder<int>> collections = new();
    private static FastReflectionDelegate m_clear;
    private static FastReflectionDelegate m_get_Count;
    private static PropertyInfo? p_capacity;
    static CollectionCache()
    {
        var m_clear_r = typeof(T).GetMethod("Clear", Type.EmptyTypes);
        if(m_clear_r == null) throw new NotSupportedException();
        m_clear = m_clear_r.CreateFastDelegate();

        var m_gc_r = typeof(T).GetProperty("Count")?.GetMethod;
        if(m_gc_r == null) throw new NotSupportedException();
        m_get_Count = m_gc_r.CreateFastDelegate();

        p_capacity = typeof(T).GetProperty("Capacity");
        if(p_capacity?.SetMethod == null) p_capacity = null;
    }
    public static T Borrow(int hopeCapacity = 0)
    {
        lock (cache)
        {
            T? n = null;
            int ni = 0;
            int ns = 9999999;
            for (int i = 0; i < cache.Count; i++)
            {
                var wr = cache[i];
                if (wr == null || !wr.TryGetTarget(out var result)) continue;
                if (hopeCapacity > 0)
                {
                    if (!collections.TryGetValue(result, out var cap)) continue;
                    var s = Mathf.Abs(cap - hopeCapacity);
                    if (s < ns)
                    {
                        n = result;
                        ni = i;
                        ns = s;
                    }
                    continue;
                }
                cache[i] = null;
                return result;
            }
            if (hopeCapacity > 0 && n != null)
            {
                cache[ni] = null;
                return n;
            }
            var r = new T();
            if (p_capacity != null)
            {
                p_capacity.FastSet(r, hopeCapacity);
            }
            collections.Add(r, new()
            {
                value = 0
            });
            return r;
        }
    }
    public static bool Recyle(T collection)
    {
        if (!collections.TryGetValue(collection, out var countHolder)) return false;
        if (p_capacity != null)
        {
            countHolder.value = (int)p_capacity.FastGet(collection)!;
        }
        else
        {
            countHolder.value = (int)m_get_Count.Invoke(collection);
        }

        m_clear.Invoke(collection);
        for (int i = 0; i < cache.Count; i++)
        {
            var wr = cache[i];
            if (wr != null && wr.TryGetTarget(out _)) continue;
            cache[i] = new(collection);
            return true;
        }
        cache.Add(new(collection));
        return true;
    }
}

public static class CollectionCacheExt
{
    public static bool RecyleCollection<T>(this T collection) where T : class ,new()
    {
        return CollectionCache<T>.Recyle(collection);
    }
}
