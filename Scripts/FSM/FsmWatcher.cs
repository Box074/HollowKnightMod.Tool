
namespace HKTool.FSM;

public class FsmWatcher : WatcherBase<FsmWatcher, PlayMakerFSM, PlayMakerFSM>
{
    static FsmWatcher()
    {
        Modding.Logger.Log("Patch PlayMakerFSM.Start");
        On.PlayMakerFSM.Start += PlayMakerFSM_Start;
    }
    public FsmWatcher(IFilter<PlayMakerFSM> filter, WatchHandler<PlayMakerFSM> handler) : base(filter, handler)
    {

    }
    protected override void CallHandler(PlayMakerFSM pm)
    {
        Handler(pm);
    }
    private static void PlayMakerFSM_Start(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
    {
        orig(self);
        Test(self);
    }

}
