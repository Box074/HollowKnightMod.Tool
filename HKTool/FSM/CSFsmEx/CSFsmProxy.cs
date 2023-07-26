using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.FSM.CSFsmEx
{
    public class CSFsmProxy : MonoBehaviour
    {
        public CSFsmHost Host { get; internal set; } = null!;

        internal void InitHost(CSFsmHost host, PlayMakerFSM fsm)
        {
            Host = host;
            host.InitHost(fsm);
        }
        public static CSFsmProxy Apply<T>(PlayMakerFSM pm) where T : CSFsmHost, new()
        {
            var inst = pm.gameObject.AddComponent<CSFsmProxy>();
            inst.InitHost(Activator.CreateInstance<T>(),pm);
            return inst;
        }
        public static PlayMakerFSM Attach<T>(GameObject go, out CSFsmProxy proxy, 
            string startState = "") where T : CSFsmHost, new()
        {
            var pm = go.AddComponent<PlayMakerFSM>();

            var t = Apply<T>(pm);
            //startState = string.IsNullOrEmpty(startState) ? t.InitState : startState;
            pm.Fsm.StartState = startState;
            pm.Fsm.Name = typeof(T).Name;
            pm.SetState(startState);
            proxy = t;
            return pm;
        }
    }
}
