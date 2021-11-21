using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HutongGames.PlayMaker;

namespace HKTool.FSM
{
    delegate void FsmCatch_Handler(FSMQuene fsm);
    static class FsmManager
    {
        private static bool _init = false;
        public readonly static List<(FsmPatchAttribute, FsmCatch_Handler)> handlers = new List<(FsmPatchAttribute, FsmCatch_Handler)>();
        public static void Init()
        {
            if (_init) return;
            _init = true;

            On.PlayMakerFSM.Start += PlayMakerFSM_Start;
        }

        private static void PlayMakerFSM_Start(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
        {
            orig(self);
            foreach (var v in handlers
                .Where(
                    x => string.IsNullOrEmpty(x.Item1.sceneName) || x.Item1.sceneName == self.gameObject.scene.name
                )
                .Where(
                    x => string.IsNullOrEmpty(x.Item1.objName) || x.Item1.objName == self.gameObject.name
                )
                .Where(
                    x => string.IsNullOrEmpty(x.Item1.fsmName) || x.Item1.fsmName == self.Fsm.Name
                )
                )
            {
                try
                {
                    var q = self.Fsm.CreateQuene();
                    v.Item2(q);
                    q.EndFSMEdit(false);
                }
                catch (Exception e)
                {
                    Modding.Logger.LogError(e);
                }
            }
        }
    }
}
