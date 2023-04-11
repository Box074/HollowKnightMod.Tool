
namespace HKTool.Utils;

public static class SpriteUtils
{
    static tk2dSprite cacheTk2d = null!;
    static SpriteRenderer cacheSpriteR = null!;
    public static Texture2D ExtractSprite(Sprite sprite)
    {
        return ExtractSprite(sprite, false);
    }
    public static Texture2D ExtractSprite(Sprite sprite, bool fixborder)
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

        width = width > 0 ? width : 1;
        height = height > 0 ? height : 1;

        cacheSpriteR.gameObject.SetActive(true);
        var tex2d = cacheSpriteR.gameObject.Render(width, height)!;
        cacheSpriteR.gameObject.SetActive(false);

        if(fixborder)
        {
            var origSize = new Vector2Int(width, height);
            var offset = Vector2Int.zero;

            var pivot = sprite.pivot / new Vector2(width, height);
            var originalPivot = new Vector2(0.5f, 0.5f);
            var delta = originalPivot - pivot;

            if(delta.x > 0)
            {
                offset.x = Mathf.RoundToInt(delta.x * 2) * width;
                origSize.x = width + offset.x;
            }
            else
            {
                origSize.x = width + Mathf.RoundToInt(-delta.x * 2) * width;
            }

            if(delta.y > 0)
            {
                offset.y = Mathf.RoundToInt(delta.y * 2) * height;
                origSize.y = height + offset.y;
            }
            else
            {
                origSize.y = height + Mathf.RoundToInt(-delta.y * 2) * height;
            }

            var otex = Texture2D.redTexture.Clone(origSize, TextureFormat.RGBA32);
            tex2d.CopyTo(otex, new RectInt(0, 0, tex2d.width, tex2d.height),
                                new RectInt(offset.x, offset.y, tex2d.width, tex2d.height));
            UObject.DestroyImmediate(tex2d);
            tex2d = otex;
        }

        return tex2d;
    }
    public static Texture2D ExtractTk2dSprite(tk2dSpriteCollectionData def, int id)
    {
        return ExtractTk2dSprite(def, id, false);
    }
    public static Texture2D ExtractTk2dSprite(tk2dSpriteCollectionData def, int id, bool fixborder)
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
            (height, width) = (width, height);
        }

        cacheTk2d.gameObject.SetActive(true);
        var tex2d = cacheTk2d.gameObject.Render(width, height)!;
        cacheTk2d.gameObject.SetActive(false);

        if (fixborder)
        {
            var trimedB = sdef.GetBounds();
            var untrimedB = sdef.GetUntrimmedBounds();

            var pixelPerUnitX = tex2d.width / trimedB.size.x;
            var pixelPerUnitY = tex2d.height / trimedB.size.y;

            var otex = Texture2D.redTexture.Clone(new((int)(untrimedB.size.x * pixelPerUnitX),
               (int)(untrimedB.size.y * pixelPerUnitY)), TextureFormat.RGBA32);
            //var otex = new Texture2D((int)(untrimedB.size.x * pixelPerUnitX),
            //   (int)(untrimedB.size.y * pixelPerUnitY), TextureFormat.RGBA32, false);

            var offsetX = (int)(Mathf.Abs(trimedB.min.x - untrimedB.min.x) * pixelPerUnitX);
            var offsetY = (int)(Mathf.Abs(trimedB.min.y - untrimedB.min.y) * pixelPerUnitY);

            tex2d.CopyTo(otex, new RectInt(0, 0, tex2d.width, tex2d.height),
                                new RectInt(offsetX, offsetY, tex2d.width, tex2d.height));
            UObject.DestroyImmediate(tex2d);
            tex2d = otex;
        }

        return tex2d;
    }
}
