using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace HKTool.FSM
{
    public class FSMQuene
    {
        internal FSMQuene()
        {

        }
        public Fsm TargetFSM { get; private set; }
        public FsmState CurrentState { get; private set; }
        public List<FsmState> States { get; private set; }
        public List<FsmTransition> Transitions { get; private set; }
        public List<FsmStateAction> Actions { get; private set; }
        public FsmStateAction LastOperationAction { get; private set; }
        private bool isClose = false;
        internal FSMQuene Init(Fsm fsm)
        {
            TargetFSM = fsm;
            States = fsm.States.ToList();
            EditState(fsm.StartState);
            return this;
        }
        ~FSMQuene()
        {
            if (!isClose)
            {
                EndFSMEdit(false);
            }
        }

        private void TestIsClose()
        {
            if (isClose) throw new InvalidOperationException();
        }
        public FSMQuene FlushState()
        {
            TestIsClose();
            if (CurrentState != null)
            {
                CurrentState.Actions = Actions.ToArray();
                CurrentState.Transitions = Transitions.ToArray();
            }
            return this;
        }
        public FSMQuene FlushFsm()
        {
            TestIsClose();
            FlushState();
            TargetFSM.States = States.ToArray();
            return this;
        }
        public void EndFSMEdit(bool gc = true)
        {
            //TestIsClose();
            if (isClose) return;
            FlushFsm();
            isClose = true;
            TargetFSM = null;
            States = null;
            Actions = null;
            Transitions = null;
            GC.SuppressFinalize(this);
            if(gc) GC.Collect(2, GCCollectionMode.Forced, false, false);
        }
        public FSMQuene EditState(string name)
        {
            TestIsClose();
            if (CurrentState != null)
            {
                FlushState();
            }
            CurrentState = States.FirstOrDefault(x => x.Name == name);
            if(Transitions == null) Transitions = CurrentState.Transitions.ToList();
            else
            {
                Transitions.Clear();
                Transitions.AddRange(CurrentState.Transitions);
            }
            if(Actions == null) Actions = CurrentState.Actions.ToList();
            else
            {
                Actions.Clear();
                Actions.AddRange(CurrentState.Actions);
            }
            return this;
        }
        public FSMQuene InsertAction(FsmStateAction action, int index)
        {
            TestIsClose();
            LastOperationAction = action;
            Actions.Insert(index, action);
            return this;
        }
        public FSMQuene AppendAction(FsmStateAction action)
        {
            TestIsClose();
            LastOperationAction = action;
            Actions.Add(action);
            return this;
        }
        public FSMQuene FindTransition(string name, Action<FsmTransition> func)
        {
            TestIsClose();
            func(Transitions.FirstOrDefault(x => x.EventName == name));
            return this;
        }

        public FSMQuene RemoveTransition(string name)
        {
            TestIsClose();
            Transitions.RemoveAll(x => x.EventName == name);
            return this;
        }

        public FSMQuene FindAction<T>(Action<T> func, int index) where T : FsmStateAction
        {
            TestIsClose();
            var action = Actions.OfType<T>().ElementAtOrDefault(index);
            LastOperationAction = action;
            func(action);
            return this;
        }

        public FSMQuene FindAction<T>(Action<T> func) where T : FsmStateAction => FindAction<T>(func, 0);
        public FSMQuene EditLastAction<T>(Action<T> func) where T : FsmStateAction
        {
            TestIsClose();
            if (LastOperationAction == null || !(LastOperationAction is T)) throw new InvalidOperationException();
            func((T)LastOperationAction);
            return this;
        }
        public FSMQuene EditLastAction(Action<FsmStateAction> func) => EditLastAction<FsmStateAction>(func);
        public FSMQuene ForEachFsmStateTransitions(ForEachFsmTransitionDelegate fun)
        {
            TestIsClose();
            for (int i = 0; i < Transitions.Count; i++)
            {
                var v = fun(Transitions[i]);
                Transitions[i] = v;
            }
            Actions.RemoveAll(x => x is null);
            return this;
        }
        public FSMQuene ForEachFsmStateActions<T>(ForEachFsmStateActionDelegate<T> fun) where T : FsmStateAction
        {
            TestIsClose();
            for (int i = 0; i < Actions.Count; i++)
            {
                var v = Actions[i] as T;
                if (v != null)
                {
                    var r = fun(v);
                    Actions[i] = r;
                }
            }
            Actions.RemoveAll(x => x is null);
            return this;
        }
        
        public FSMQuene AddState(string name)
        {
            TestIsClose();
            var s = new FsmState(TargetFSM)
            {
                Name = name
            };
            States.Add(s);
            return this;
        }
        public FSMQuene AddStateAndEdit(string name)
        {
            var s = new FsmState(TargetFSM)
            {
                Name = name
            };
            States.Add(s);
            EditState(name);
            return this;
        }
    }
}
