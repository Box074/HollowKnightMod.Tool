
namespace HKTool.Utils;

public static class RendererUtils
{
    static Camera cacheCamera = null!;
    private static void PrepareCamera()
    {
        var camGO = new GameObject("HKTool Cache Camera");
        cacheCamera = camGO.AddComponent<Camera>();
        cacheCamera.orthographic = true;
        cacheCamera.clearFlags = CameraClearFlags.Nothing;
        UnityEngine.Object.DontDestroyOnLoad(camGO);
    }
    private static void BuildCamera(UnityEngine.Bounds bounds)
    {
        if (cacheCamera == null) PrepareCamera();

        cacheCamera!.orthographicSize = bounds.size.y / 2;
        cacheCamera!.aspect = bounds.size.x / bounds.size.y;
        cacheCamera!.transform.position = bounds.center.With((ref Vector3 x) => x.z = bounds.min.z - 1);
    }
    public static Texture2D? Render(this GameObject go, int width, int height)
    {
        return Render(go, width, height, false);
    }
    public static Texture2D? Render(this GameObject go, int width, int height, bool includeChildren)
    {
        var render = go.GetComponent<Renderer>();
        if (render == null) return null;
        var origPos = go.transform.position;
        var origE = render.enabled;
        Renderer[] enabled = go.GetComponentsInChildren<Renderer>(false).Where(x => x.enabled && x.gameObject.activeInHierarchy).ToArray();
        if (!includeChildren)
        {
            
            go.transform.position = go.transform.position.With((ref Vector3 x) => x.z = -999999);
            foreach (var v in enabled) v.enabled = false;
        }
        render.enabled = true;
        UnityEngine.Bounds bounds = new();
        if(!includeChildren)
        {
            bounds = render.bounds;
        }
        else
        {
            bounds.min = new(enabled.Select(x => x.bounds.min.x).Min(), enabled.Select(x => x.bounds.min.y).Min(), render.bounds.min.z);
            bounds.max = new(enabled.Select(x => x.bounds.max.x).Max(), enabled.Select(x => x.bounds.max.y).Max(), render.bounds.max.z);
        }
        BuildCamera(bounds);

        var rtex = new RenderTexture(width, height, 0);
        rtex.Create();
        cacheCamera.targetTexture = rtex;
        cacheCamera.Render();
        go.transform.position = origPos;
        render.enabled = origE;
        if (!includeChildren)
        {
            foreach (var v in enabled) v.enabled = true;
        }
        var tex2d = new Texture2D(width, height);
        var prev = RenderTexture.active;
        RenderTexture.active = rtex;
        tex2d.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex2d.Apply();
        RenderTexture.active = prev;

        rtex.Release();
        return tex2d;
    }
}
