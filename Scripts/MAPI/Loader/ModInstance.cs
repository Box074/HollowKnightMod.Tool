
namespace HKTool.MAPI.Loader;

public sealed class ModInstance
{
    private static Type TModInstance = FindType("Modding.ModLoader+ModInstance")!;
    private static Type TModErrorState = FindType("Modding.ModLoader+ModErrorState")!;
    private static FieldInfo TModInstanceMod = FindFieldInfo("Modding.ModLoader+ModInstance::Mod");
    private static FieldInfo TModInstanceName = FindFieldInfo("Modding.ModLoader+ModInstance::Name");
    private static FieldInfo TModInstanceError = FindFieldInfo("Modding.ModLoader+ModInstance::Error");
    private static FieldInfo TModInstanceEnabled = FindFieldInfo("Modding.ModLoader+ModInstance::Enabled");
	public IMod? Mod
    {
        get => (IMod?)TModInstanceMod.FastGet(bindMi);
        set => TModInstanceMod.FastSet(bindMi, value);
    }
	public string? Name
    {
        get => (string?)TModInstanceName.FastGet(bindMi);
        set => TModInstanceName.FastSet(bindMi, value);
    }
	public ModErrorState? Error
    {
        get => (ModErrorState?)((IConvertible?)TModInstanceError.FastGet(bindMi))?.ToInt32(null);
        set => 
            TModInstanceError.FastSet(bindMi, value != null ? Enum.Parse(TModErrorState, value.ToString()) : null);
    }
	public bool Enabled
    {
        get => (bool)TModInstanceEnabled.FastGet(bindMi)!;
        set => TModInstanceEnabled.FastSet(bindMi, value);
    }
    private object bindMi;
    public ModInstance()
    {
        bindMi = Activator.CreateInstance(TModInstance);
    }
    public ModInstance(object mi)
    {
        bindMi = mi;
    }
    public object Get()
    {
        return bindMi;
    }
}
