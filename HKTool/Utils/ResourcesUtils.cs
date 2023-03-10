
namespace HKTool;

public static class ResourcesUtils
{
    private static Dictionary<Type, Dictionary<string, WeakReference<UObject>>> cache = new();
    public static string GetMatchName(UObject obj)
    {
        {
            var ox = obj;
            var t = obj.GetType();
            if (t.IsSubclassOf(typeof(Component)))
            {
                obj = ((Component)ox).gameObject;
                t = typeof(GameObject);
            }
            if (t == typeof(GameObject))
            {
                var go = (GameObject)obj;
                if (go.transform.parent == null)
                {
                    return go.name;
                }
                else
                {
                    return go.GetPath();
                }
            }
            return ox.name;
        }
    }
    public static Dictionary<string, UObject?> FindAssets(IEnumerable<string> names, Type type)
    {
        Dictionary<string, UObject?> dict = new();
        foreach(var name in names)
        {
            var result = FindAsset(name, type);
            dict[name] = result;
        }
        return dict;
    }
    public static UObject? FindAsset(string name, Type type)
    {
        var cacheT = cache.TryGetOrAddValue(type, () => new());
        UObject? result = null;
        if (cacheT.TryGetValue(name, out var objr) && (objr?.TryGetTarget(out result) ?? false)) return result;

        IEnumerable<(UObject obj, string matchname)> assets = Resources.FindObjectsOfTypeAll(type)
                                                                            .Where(x => (x?.GetInstanceID() ?? 0) > 0)
                                                                            .Select(x => (x, GetMatchName(x)));
        foreach (var (obj, matchname) in assets)
        {
            if (obj is GameObject go && go.scene.IsValid()) continue;
            cacheT[matchname] = new(obj);
            if(GetMatchName(obj) == name)
            {
                result = obj;
            }
        }
        return result;
    }
    public static T? FindAsset<T>(string name) where T : UObject => (T?)FindAsset(name, typeof(T));
}

