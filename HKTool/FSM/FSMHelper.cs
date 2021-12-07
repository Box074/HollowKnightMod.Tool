using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HutongGames.PlayMaker;
using UnityEngine;

namespace HKTool.FSM
{
    public delegate FsmStateAction ForEachFsmStateActionDelegate<T>(T action) where T : FsmStateAction;
    public delegate FsmTransition ForEachFsmTransitionDelegate(FsmTransition transition);
    public static class FSMHelper
    {
        public static Fsm[] FindFsms(string name)
        {
            return PlayMakerFSM.FsmList.Select(x => x.Fsm).Where(x => x.Name == name).ToArray();
        }
        public static Fsm FindFsm(string name)
        {
            return PlayMakerFSM.FsmList.FirstOrDefault(x => x.Fsm.Name == name)?.Fsm;
        }
        public static T[] GetFSMStateActionsOnState<T>(this FsmState state) where T : FsmStateAction
        {
            return state.Actions.Where(x => typeof(T).IsAssignableFrom(x.GetType())).Cast<T>().ToArray();
        }
        public static T[] GetFSMStateActionsOnFSM<T>(this Fsm fsm) where T : FsmStateAction
        {
            return fsm.States.SelectMany(x => GetFSMStateActionsOnState<T>(x)).ToArray();
        }
        public static T GetFSMStateActionOnState<T>(this FsmState state) where T : FsmStateAction
        {
            return GetFSMStateActionsOnState<T>(state).FirstOrDefault();
        }
        public static T GetFSMStateActionOnFSM<T>(this Fsm fsm) where T : FsmStateAction
        {
            return GetFSMStateActionsOnFSM<T>(fsm).FirstOrDefault();
        }
        public static T InsertFsmStateAction<T>(this FsmState state, T action, int index) where T : FsmStateAction
        {
            var l = state.Actions.ToList();
            l.Insert(index, action);
            state.Actions = l.ToArray();
            return action;
        }
        public static T AppendFsmStateAction<T>(this FsmState state, T action) where T : FsmStateAction
        {
            state.Actions = state.Actions.Append(action).ToArray();
            return action;
        }
        public static void RemoveAllFsmStateActions<T>(this FsmState state) where T : FsmStateAction
        {
            state.Actions = state.Actions.Where(x => !(x is T)).ToArray();
        }
        public static void ForEachFsmStateAction<T>(this FsmState state, ForEachFsmStateActionDelegate<T> fun) where T : FsmStateAction
        {
            var l = new List<FsmStateAction>(state.Actions.Length);
            foreach(var v in state.Actions)
            {
                var t = v;
                if(t is T o)
                {
                    t = fun(o);
                    if (t != null) l.Add(t);
                }
                else
                {
                    l.Add(v);
                }
            }
            state.Actions = l.ToArray();
        }
        public static FSMPatch CreatePatch(this Fsm fsm)
        {
            return new FSMPatch().Init(fsm);
        }
        public static FsmState InsertFSMState(this Fsm fsm, FsmState state)
        {
            fsm.States = fsm.States.Append(state).ToArray();
            return state;
        }
        public static FsmState GetFSMState(this GameObject go, string state, string fsm = null)
        {
            if (!string.IsNullOrEmpty(fsm))
            {
                return PlayMakerFSM.FsmList.Where(x => x.gameObject == go)?.FirstOrDefault(x => x.name == fsm)
                    .Fsm.GetState(state);
            }
            else
            {
                return PlayMakerFSM.FsmList.Where(x => x.gameObject == go)
                    .SelectMany(x => x.FsmStates)
                    .FirstOrDefault(x => x.Name == state);
            }
        }
        public static void ForEachFsmStateTransition(this FsmState state, ForEachFsmTransitionDelegate fun)
        {
            var l = new List<FsmTransition>(state.Transitions.Length);
            foreach (var v in state.Transitions)
            {
                var t = fun(v);
                if (t != null)
                {
                    l.Add(t);
                }
            }
            state.Transitions = l.ToArray();
        }
        public static void AppendFsmStateTransition(this FsmState state, FsmTransition transition)
        {
            state.Transitions = state.Transitions.Append(transition).ToArray();
        }
        public static FsmTransition FindFsmStateTransition(this FsmState state, string name)
        {
            return state.Transitions.FirstOrDefault(x => x.FsmEvent.Name == name);
        }
        public static FsmTransition FindOrCreateFsmStateTransition(this FsmState state, FsmEvent e)
        {
            var r = state.Transitions.FirstOrDefault(x => x.FsmEvent.Name == e.Name);
            if (r != null) return r;
            r = new FsmTransition()
            {
                FsmEvent = e
            };
            AppendFsmStateTransition(state, r);
            return r;
        }
        public static Fsm GetFsm(this GameObject go, string name) => go.LocateMyFSM(name).Fsm;
        public static FsmStateAction CreateMethodAction(Action<FsmStateAction> action)
        {
            return new MethodFSMStateAction(action);
        }
        public static void RegisterFSMPatcher(FsmFilter filter, FsmPatchHandler patcher)
        {
            FsmManager.RegisterPatcher(filter, patcher);
        }
        class MethodFSMStateAction : FsmStateAction
        {
            public Action<FsmStateAction> action;
            public MethodFSMStateAction(Action<FsmStateAction> action)
            {
                this.action = action;
            }
            public override void OnEnter()
            {
                action(this);
                Finish();
            }
        }
    }
}
