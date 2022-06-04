
namespace HKTool.Runtime;

public interface IHKToolMod
{
    void HookInit(PreloadObject objs, PreloadAsset assets);
}
