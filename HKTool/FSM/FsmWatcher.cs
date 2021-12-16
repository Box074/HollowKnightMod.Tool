using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HutongGames.PlayMaker;

namespace HKTool.FSM
{
    public delegate void FsmPatchHandler(FSMPatch fsm);
    
    public class FsmWatcher : IWatcher<PlayMakerFSM>
    {
        private readonly static List<FsmWatcher> watchers = new List<FsmWatcher>();
        static FsmWatcher()
        {
            On.PlayMakerFSM.Start += PlayMakerFSM_Start;
        }
        public IFsmFilter Filter { get; set; }
        public FsmPatchHandler Handler { get; set; }
        public FsmWatcher(IFsmFilter filter, FsmPatchHandler handler)
        {
            Filter = filter;
            Handler = handler;
            watchers.Add(this);
        }
        public void RemoveWatcher()
        {
            watchers.Remove(this);
        }
        public void Try(PlayMakerFSM pm)
        {
            if (Handler == null || Filter == null) return;
            try
            {
                if (Filter.Filter(pm))
                {
                    using (var p = pm.Fsm.CreatePatch()) Handler(p);
                }
            }catch(Exception e)
            {
                Modding.Logger.LogError(e);
            }
        }
        private static void PlayMakerFSM_Start(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
        {
            orig(self);
            foreach(var v in watchers)
            {
                v.Try(self);
            }
        }
    }
}
