
namespace HKTool.Utils;


public static class GameObjectHelper
{
    private static Lazy<GameObject> prefabHolder = new(() => {
        var go = new GameObject("HKTool Prefab Holder");
        UObject.DontDestroyOnLoad(go);
        go.SetActive(false);
        return go;
    });
    public static GameObject PrefabHolder => prefabHolder.Value;
    public static GameObject CloneAsPrefab(this GameObject go)
    {
        var result = UObject.Instantiate(go, PrefabHolder.transform);
        if(!result.activeSelf) result.SetActive(true);
        result.name = go.name;
        return result;
    }
    public static Component CloneAsPrefab(this Component c)
    {
        var result = UObject.Instantiate(c, PrefabHolder.transform);
        if(!result.gameObject.activeSelf) result.gameObject.SetActive(true);
        result.gameObject.name = c.gameObject.name;
        return result;
    }
    public static IEnumerable<GameObject> ForEachChildren(this GameObject parent)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        foreach (var v in parent.GetComponentsInChildren<Transform>(true)) yield return v.gameObject;
    }
    public static GameObject FindChild(this GameObject parent, string name)
    {
        return parent.ForEachChildren().FirstOrDefault(x => x.name == name);
    }
    public static IEnumerable<GameObject> ForEachGameObjects(this Scene scene)
    {
        foreach (var v in scene.GetRootGameObjects())
        {
            yield return v;
            foreach (var v2 in v.ForEachChildren()) yield return v2;
        }
    }
    public static GameObject? FindGameObject(this Scene scene, string name)
    {
        var p = name.Split('/');
        if (p.Length == 1)
        {
            return scene.ForEachGameObjects().FirstOrDefault(
                x => x.name.Equals(name, StringComparison.Ordinal));
        }
        var c = scene.GetRootGameObjects().FirstOrDefault(
                x => x.name.Equals(p[0], StringComparison.Ordinal));
        for (int i = 1; i < p.Length; i++)
        {
            c = c.transform.Find(p[i])?.gameObject;
            if (c == null) return null;
        }
        return c;
    }
    public static GameObject? FindChildWithPath(this GameObject root, params string[] relativePath)
    {
        var c = root;
        foreach (var v in relativePath)
        {
            c = c.transform.Find(v)?.gameObject;
            if (c == null) return null;
        }
        return c;
    }
    public static string GetPath(this GameObject go)
    {
        StringBuilder sb = new StringBuilder();
        bool first = true;
        while (go != null)
        {
            if (!first)
            {
                sb.Insert(0, '/');
            }
            sb.Insert(0, go.name);
            go = go.transform.parent?.gameObject!;
            first = false;
        }
        return sb.ToString();
    }

}
