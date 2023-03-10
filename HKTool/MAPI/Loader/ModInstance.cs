
using static Modding.ModLoaderR;

namespace HKTool.MAPI.Loader;

[Obsolete]
public sealed class ModInstance
{
	public IMod? Mod
    {
        get => bindMi.Mod.ToOriginal();
        set => bindMi.Mod = value.Reflect();
    }
	public string? Name
    {
        get => bindMi.Name;
        set => bindMi.Name = value;
    }
	public ModErrorState? Error
    {
        get => (ModErrorState?)bindMi.Error;
        set => bindMi.Error = value.HasValue ? (ModErrorStateR)value.Value : null;
    }
	public bool Enabled
    {
        get => bindMi.Enabled;
        set => bindMi.Enabled = value;
    }
    private ModInstanceR bindMi;
    public ModInstance()
    {
        bindMi = new();
    }
    public ModInstance(object mi)
    {
        bindMi = (ModInstanceR)mi;
    }
    public object Get()
    {
        return bindMi;
    }
}
