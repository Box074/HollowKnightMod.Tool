
namespace HKTool.Utils;

public static class SpriteUtils
{
    static tk2dSprite cacheTk2d = null!;
    static SpriteRenderer cacheSpriteR = null!;
    public static Texture2D ExtractSprite(Sprite sprite)
    {
        if (cacheSpriteR == null)
        {
            var renderGO = new GameObject("Sprite Renderer");
            renderGO.transform.position = new(0, 0, 55555);
            UObject.DontDestroyOnLoad(renderGO);
            cacheSpriteR = renderGO.AddComponent<SpriteRenderer>();
        }
        cacheSpriteR.sprite = sprite;

        var width = (int)(sprite.bounds.size.x * sprite.pixelsPerUnit);
        var height = (int)(sprite.bounds.size.y * sprite.pixelsPerUnit);

        cacheSpriteR.gameObject.SetActive(true);
        var tex2d = cacheSpriteR.gameObject.Render(width, height)!;
        cacheSpriteR.gameObject.SetActive(false);
        return tex2d;
    }
    public static Texture2D ExtractTk2dSprite(tk2dSpriteCollectionData def, int id)
    {
        var sdef = def.spriteDefinitions[id];
        
        if (cacheTk2d == null)
        {
            var renderGO = new GameObject("Tk2d Renderer", typeof(MeshRenderer), typeof(MeshFilter));
            renderGO.transform.position = new(0, 0, 55555);
            UObject.DontDestroyOnLoad(renderGO);
            cacheTk2d = tk2dSprite.AddComponent(renderGO, def, id);
        }
        cacheTk2d.SetSprite(def, id);
        var width = (int)((sdef.uvs.Max(x => x.x) - sdef.uvs.Min(x => x.x)) * sdef.material.mainTexture.width) + 1;
        var height = (int)((sdef.uvs.Max(x => x.y) - sdef.uvs.Min(x => x.y)) * sdef.material.mainTexture.height) + 1;
        if (sdef.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
        {
            var o = width;
            width = height;
            height = o;
        }

        cacheTk2d.gameObject.SetActive(true);
        var tex2d = cacheTk2d.gameObject.Render(width, height)!;
        cacheTk2d.gameObject.SetActive(false);

        return tex2d;
    }
}
