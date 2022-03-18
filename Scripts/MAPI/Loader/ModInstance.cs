
namespace HKTool.MAPI.Loader;

public class ModInstance
{
    public static Type TModInstance = ModLoaderHelper.TModLoader.GetNestedType("ModInstance");
    public static Type TModErrorState = ModLoaderHelper.TModLoader.GetNestedType("ModErrorState");
	public IMod Mod;
	public string Name;
	public ModErrorState? Error;
	public bool Enabled;
    private object bindMi = null;
    public ModInstance()
    {

    }
    public ModInstance(object mi)
    {
        Read(mi);
    }
    public void Read(object src)
    {
        bindMi = src;
        var r = src.CreateReflectionObject();
        Mod = r["Mod"].As<IMod>();
        Name = r["Name"].As<string>();
        Enabled = r["Enabled"].As<bool>();
        var err = r["Error"];
        if(err is null)
        {
            Error = null;
        }
        else
        {
            Error = (ModErrorState)((IConvertible)err.As<Enum>()).ToInt32(null);
        }
    }
    public void Write(object dst = null)
    {
        if(dst is null) dst = bindMi;
        var r = dst.CreateReflectionObject();
        r.SetMemberData("Mod", Mod);
        r.SetMemberData("Name", Name);
        r.SetMemberData("Enabled", Enabled);
        if(!Error.HasValue)
        {
            r.SetMemberData("Error", null);
        }
        else
        {
            r.SetMemberData("Error", Enum.Parse(TModErrorState, Error.Value.ToString()));
        }
    }
    public object New()
    {
        var mi = Activator.CreateInstance(TModInstance);
        if(bindMi is null) bindMi = mi;
        Write(mi);
        return mi;
    }
}
