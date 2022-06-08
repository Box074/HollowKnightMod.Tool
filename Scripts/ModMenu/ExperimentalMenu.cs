
namespace HKTool.ModMenu;

class ExperimentalMenu : CustomMenu
{
    public static ExperimentalMenu instance = null!;
    public ExperimentalMenu(MenuScreen rs) : base(rs, "HKTool.Experimental.Title".Localize())
    {

    }
    private void HasSatchel()
    {
        Texture2D HookETFS(On.Satchel.SpriteUtils.orig_ExtractTextureFromSprite orig, object testSprite, bool saveTriangles)
        {
            return Satchel.SpriteUtils.ExtractTextureFromSpriteExperimental((Sprite)testSprite, saveTriangles);
        }
        if (HKToolMod.settings.ExperimentalConfig.satchel_use_shader_extract)
        {
            On.Satchel.SpriteUtils.ExtractTextureFromSprite += HookETFS;
        }

        AddBoolOption("satchel use shader extract", "",
        ref HKToolMod.settings.ExperimentalConfig.satchel_use_shader_extract,
        () =>
        {

            if (HKToolMod.settings.ExperimentalConfig.satchel_use_shader_extract)
            {
                On.Satchel.SpriteUtils.ExtractTextureFromSprite += HookETFS;
            }
            else
            {
                On.Satchel.SpriteUtils.ExtractTextureFromSprite -= HookETFS;
            }
        });
    }
    protected override void Build(ContentArea contentArea)
    {
        try
        {
            HasSatchel();
        }
        catch(Exception)
        {

        }
    }
}
