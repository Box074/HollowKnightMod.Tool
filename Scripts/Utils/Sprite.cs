
namespace HKTool.Utils;

public static class SpriteUtils
{
    static Camera cacheCamera = null!;
    static tk2dSprite cacheTk2d = null!;
    static SpriteRenderer cacheSpriteR = null!;
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
    public static Texture2D ExtractSprite(Sprite sprite)
    {
        if (cacheSpriteR == null)
        {
            var renderGO = new GameObject("Sprite Renderer");
            renderGO.transform.position = new(0, 0, 55555);
            UnityEngine.Object.DontDestroyOnLoad(renderGO);
            cacheSpriteR = renderGO.AddComponent<SpriteRenderer>();
        }
        cacheSpriteR.sprite = sprite;

        BuildCamera(cacheSpriteR.bounds);

        var width = (int)(sprite.bounds.size.x * sprite.pixelsPerUnit);
        var height = (int)(sprite.bounds.size.y * sprite.pixelsPerUnit);

        var rtex = new RenderTexture(width, height, 0);
        rtex.Create();
        cacheCamera.targetTexture = rtex;
        cacheSpriteR.gameObject.SetActive(true);
        cacheCamera.Render();
        cacheSpriteR.gameObject.SetActive(false);

        var tex2d = new Texture2D(width, height);
        var prev = RenderTexture.active;
        RenderTexture.active = rtex;
        tex2d.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex2d.Apply();
        RenderTexture.active = prev;

        rtex.Release();
        return tex2d;
    }
    public static Texture2D ExtractTk2dSprite(tk2dSpriteCollectionData def, int id)
    {
        var sdef = def.spriteDefinitions[id];

        if (cacheTk2d == null)
        {
            var renderGO = new GameObject("Tk2d Renderer", typeof(MeshRenderer), typeof(MeshFilter));
            renderGO.transform.position = new(0, 0, 55555);
            UnityEngine.Object.DontDestroyOnLoad(renderGO);
            cacheTk2d = tk2dSprite.AddComponent(renderGO, def, id);
        }
        cacheTk2d.SetSprite(def, id);

        var mr = cacheTk2d.GetComponent<MeshRenderer>();
        BuildCamera(mr.bounds);

        var width = (int)((sdef.uvs.Max(x => x.x) - sdef.uvs.Min(x => x.x)) * sdef.material.mainTexture.width) + 1;
        var height = (int)((sdef.uvs.Max(x => x.y) - sdef.uvs.Min(x => x.y)) * sdef.material.mainTexture.height) + 1;
        if (sdef.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
        {
            var o = width;
            width = height;
            height = o;
        }
        var rtex = new RenderTexture(width, height, 0);
        rtex.Create();
        cacheCamera.targetTexture = rtex;
        cacheTk2d.gameObject.SetActive(true);
        cacheCamera.Render();
        cacheTk2d.gameObject.SetActive(false);

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
