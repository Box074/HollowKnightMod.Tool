using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HutongGames.PlayMaker;

namespace HKTool.FSM
{
    public delegate void FsmPatchHandler(FSMPatch fsm);
    
    static class FsmManager
    {
        private static bool _init = false;
        public readonly static List<(FsmFilter, FsmPatchHandler)> handlers = new List<(FsmFilter, FsmPatchHandler)>();
        public static void Init()
        {
            if (_init) return;
            _init = true;

            On.PlayMakerFSM.Start += PlayMakerFSM_Start;
        }
        public static void RegisterPatcher(FsmFilter filter, FsmPatchHandler handler)
        {
            if (filter is null) throw new ArgumentNullException(nameof(filter));
            if (handler is null) throw new ArgumentNullException(nameof(handler));
            handlers.Add((filter, handler));
        }
        private static IEnumerable<FsmPatchHandler> FindHandlers(PlayMakerFSM pm)
        {
            bool flag;
            foreach(var v in handlers)
            {
                flag = false;
                try
                {
                    flag = v.Item1.Filter(pm);
                }catch(Exception e)
                {
                    Modding.Logger.LogError(e);
                }
                if (flag) yield return v.Item2;
            }
        }
        private static void PlayMakerFSM_Start(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
        {
            orig(self);
            foreach (var v in FindHandlers(self))
            {
                var q = self.Fsm.CreatePatch();
                try
                {
                    v(q);
                }
                catch (Exception e)
                {
                    Modding.Logger.LogError(e);
                }
                finally
                {
                    q.EndFSMEdit();
                }
            }
        }
    }
}
