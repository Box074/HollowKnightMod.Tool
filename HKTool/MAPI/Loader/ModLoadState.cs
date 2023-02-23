
namespace HKTool.MAPI.Loader;

[Flags]
public enum ModLoadState
{
    NotStarted = 0,
    Started = 1,
    Preloaded = 2,
    Loaded = 4,
}
