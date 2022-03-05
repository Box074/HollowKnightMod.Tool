
namespace HKTool.FSM;
public abstract class FsmFilter : IFsmFilter
{
    public abstract bool Filter(PlayMakerFSM pm);
}
public class FsmNameFilterRegex : FsmFilter
{
    public readonly Regex sceneName;
    public readonly Regex objName;
    public readonly Regex fsmName;
    public FsmNameFilterRegex(Regex sceneName = null, Regex objName = null, Regex fsmName = null)
    {
        this.sceneName = sceneName;
        this.objName = objName;
        this.fsmName = fsmName;
    }
    public override bool Filter(PlayMakerFSM pm)
    {
        if (!(sceneName?.IsMatch(pm.gameObject.scene.name) ?? true)) return false;
        if (!(objName?.IsMatch(pm.gameObject.name) ?? true)) return false;
        if (!(fsmName?.IsMatch(pm.Fsm.Name) ?? true)) return false;
        return true;
    }
}
public class FsmNameFilter : FsmFilter
{
    public readonly string sceneName;
    public readonly string objName;
    public readonly string fsmName;
    public FsmNameFilter(string sceneName = null, string objName = null, string fsmName = null)
    {
        this.sceneName = sceneName;
        this.objName = objName;
        this.fsmName = fsmName;
    }
    public override bool Filter(PlayMakerFSM pm)
    {
        if (pm.gameObject.scene.name != sceneName && !string.IsNullOrEmpty(sceneName)) return false;
        if (pm.gameObject.name != objName && !string.IsNullOrEmpty(objName)) return false;
        if (pm.Fsm.Name != fsmName && !string.IsNullOrEmpty(fsmName)) return false;
        return true;
    }
}
public class LambdaFsmFilter : FsmFilter
{
    private readonly Predicate<PlayMakerFSM> test;
    public override bool Filter(PlayMakerFSM pm)
    {
        return test?.Invoke(pm) ?? false;
    }
    public LambdaFsmFilter(Predicate<PlayMakerFSM> handler)
    {
        test = handler;
    }
}

