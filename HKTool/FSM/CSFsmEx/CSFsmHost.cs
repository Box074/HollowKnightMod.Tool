using HKTool.FSM.CSFsmEx.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.FSM.CSFsmEx
{
    public class CSFsmStateMetadata
    {
        public readonly List<FsmStateAction> beforeActions = new();
        public readonly List<FsmStateAction> afterActions = new();
        public readonly IReadOnlyCollection<FsmStateAction> originalActions;
        public readonly List<(FsmEvent, string)> transitions = new();
        public readonly CSFsmHost host;
        public readonly FsmState state;
        internal CSFsmHost.StateExcuterAction excuterAction = null!;
        internal ResettableEnumerator? template = null!;
        public CSFsmStateMetadata(CSFsmHost host, FsmState state, 
            IReadOnlyCollection<FsmStateAction> originalActions)
        {
            this.host = host;
            this.state = state;
            this.originalActions = originalActions;
        }
    }
    public abstract class CSFsmHost
    {
        [AttributeUsage(AttributeTargets.Method)]
        internal protected class FsmStateAttribute : Attribute
        {
            public FsmStateAttribute()
            {

            }
            public FsmStateAttribute(string name)
            {
                this.name = name;
            }
            internal string name = "";
        }
        [AttributeUsage(AttributeTargets.Field)]
        internal protected class FsmVarAttribute : Attribute
        {
            public string varName;
            public FsmVarAttribute([CallerMemberName] string name = "")
            {
                varName = name;
            }
        }
        [AttributeUsage(AttributeTargets.Field)]
        internal protected class BindingAttribute : Attribute
        {
            public string childPath;
            public BindingAttribute(string childPath = "")
            {
                this.childPath = childPath;
            }
        }
        [AttributeUsage(AttributeTargets.Parameter)]
        internal protected class FsmTransitionAttribute : Attribute
        {
            public string eventName;
            public string targetName;
            public FsmTransitionAttribute(string targetName,string eventName = "")
            {
                this.eventName = eventName;
                this.targetName = targetName;
            }
        }
        [AttributeUsage(AttributeTargets.Method)]
        internal protected class GlobalEventAttribute : Attribute
        {
            public string eventName;
            public GlobalEventAttribute(string eventName)
            {
                this.eventName = eventName;
            }
        }
        [AttributeUsage(AttributeTargets.Parameter)]
        internal protected class GetOrAddAttribute : Attribute
        { }
        internal protected sealed class StateInitToken
        { }

        private static Dictionary<Type, Action<CSFsmBuildContext>> builders = new();

        private CSFsmStateMetadata? m_currentState;

        private readonly List<CSFsmStateMetadata> m_states = new();

        internal CSFsmStateMetadata CurrentStateMetadata
        {
            get
            {
                return m_currentState ?? throw new NotSupportedException();
            }
        }
        [field: Binding]
        public GameObject GameObject { get; } = null!;
        [field: Binding]
        public Transform Transform { get; } = null!;
        public PlayMakerFSM Fsm { get; private set; } = null!;

        internal void InitHost(PlayMakerFSM fsm)
        {
            Fsm = fsm;

            if(!builders.TryGetValue(GetType(), out var builder))
            {
                var md = new DynamicMethodDefinition($"$CSFsmHostBuilder<{GetType().FullName}>", typeof(void), new Type[] { typeof(CSFsmBuildContext) });
                var compiler = new CSFsmCompiler();
                compiler.Compile(GetType(), md.Definition);
                //md.Module.Assembly.Write(@"C:\Users\29676\Desktop\A2.dll"); ////FIX ME!!!!!!!!!!!!!!!!!!!!!!!!!
                var mi = DMDCecilGenerator.Generate(md);
                builder = mi.CreateDelegate<Action<CSFsmBuildContext>>();
                builders[GetType()] = builder;
            }

            builder(new()
            {
                Host = this,
                Fsm = fsm.Fsm
            });

            FixTransitions();
        }
        internal void InitState(string stateName)
        {
            var original = Fsm.Fsm.GetState("[Original State]" + stateName);
            var state = Fsm.Fsm.GetState(stateName);

            if(original == null && state != null)
            {
                original = state;
                state = new(Fsm.Fsm);
                original.Name = "[Original State]" + stateName;
            }

            state ??= new(Fsm.Fsm);

            state.Name = stateName;

            Fsm.Fsm.InsertFSMState(state);

            FsmStateAction[] originalActions;
            if(original != null && original.ActionData != null)
            {
                originalActions = original.ActionData.LoadActions(original);
            }
            else
            {
                originalActions = new FsmStateAction[0];
            }
            m_currentState = new CSFsmStateMetadata(this, state, originalActions);
        }
        internal void BuildState(IEnumerator template, bool hasInit)
        {
            var cs = CurrentStateMetadata;

            if(hasInit)
            {
                template.MoveNext();
                if (template.Current != CurrentStateMetadata) throw new InvalidOperationException();
            }

            cs.template = template.AsResettable();
            cs.state.Actions = cs.beforeActions
                .Append(new StateExcuterAction(cs))
                .Concat(cs.afterActions).ToArray();
            cs.state.Transitions = cs.transitions.Select(x => new FsmTransition()
            {
                FsmEvent = x.Item1,
                ToState = x.Item2
            }).ToArray();

            m_states.Add(cs);

            m_currentState = null;
        }
        internal void FixTransitions()
        {
            foreach(var s in Fsm.Fsm.States)
            {
                foreach(var t in s.Transitions)
                {
                    if (string.IsNullOrEmpty(t.ToState)) continue;
                    t.ToFsmState = Fsm.Fsm.GetState(t.ToState);
                    if(t.ToFsmState == null)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }

        protected void InvokeAction(FsmStateAction action)
        {
            var state = Fsm.Fsm.ActiveState;
            action.Active = true;
            action.Finished = false;
            action.Init(state);
            action.Entered = true;
            action.OnEnter();

            if (!action.Finished)
            {
                state.ActiveActions.Add(action);
            }
        }

        protected void InvokeActions(FsmStateAction[] actions, int index, int count)
        {
            if (count > actions.Length) count = actions.Length;

            for (int i = 0; i < count; i++)
            {
                var action = actions[i + index];
                InvokeAction(action);
            }
        }
        protected void InvokeActions(FsmStateAction[] actions, int index = 0)
        {
            InvokeActions(actions, index, actions.Length - index);
        }

        protected IEnumerator InvokeActionAndWait(FsmStateAction action)
        {
            InvokeAction(action);
            while (!action.Finished) yield return null;
        }
        protected FsmEvent GetDelegateEvent(Action callback)
        {
            return CurrentStateMetadata.excuterAction.CatchEvent(callback);
        }
        internal class StateExcuterAction : FsmStateAction
        {
            public StateExcuterAction(CSFsmStateMetadata state)
            {
                m_state = state;
                m_host = state.host;

                state.excuterAction = this;
            }
            private CSFsmStateMetadata m_state;
            private CoroutineInfo? m_currentEx;
            private CSFsmHost m_host;
            private Dictionary<FsmEvent, Action> eventCatcher = new();
            public FsmEvent CatchEvent(Action callback)
            {
                var ev = new FsmEvent(Guid.NewGuid().ToString());
                eventCatcher[ev] = callback;
                return ev;
            }
            public override void OnEnter()
            {
                m_host.m_currentState = m_state;

                m_currentEx = ExecuteAction().CreateCoroutine();
                m_currentEx.onFinished += (_) =>
                {
                    if (m_currentEx is null) return;
                    Fsm.Event(FsmEvent.Finished);
                };
                m_currentEx.onException += (_, e) =>
                {
                    HKToolMod2.logger.LogError(e);
                };
                m_currentEx.customResult = (ref object? result) =>
                {
                    if (result is string eventName)
                    {
                        Fsm.Event(FsmEvent.GetFsmEvent(eventName));
                        result = null;
                        return true;
                    }
                    else if (result is FsmEvent eve)
                    {
                        Fsm.Event(eve);
                        result = null;
                        return true;
                    }
                    else if (result is FsmStateAction action)
                    {
                        m_host.InvokeAction(action);
                        return false;
                    }
                    else if (result is FsmStateAction[] actions)
                    {
                        m_host.InvokeActions(actions);
                        return false;
                    }
                    else if (result is ValueTuple<float, string>(var delay, var ev))
                    {
                        Fsm.DelayedEvents.Add(new(Fsm, FsmEvent.GetFsmEvent(ev), delay));
                        Fsm.UpdateDelayedEvents();
                    }
                    return true;
                };
                m_currentEx.Start();
            }
            public override void OnExit()
            {
                if(m_host.m_currentState == m_state)
                {
                    m_host.m_currentState = null;
                }
                m_currentEx?.Stop();
                m_currentEx = null;
                eventCatcher.Clear();
            }
            public override bool Event(FsmEvent fsmEvent)
            {
                if(eventCatcher.TryGetValue(fsmEvent, out var callback))
                {
                    callback();
                    return true;
                }
                return false;
            }
            private IEnumerator ExecuteAction()
            {
                var ie = m_state.template;
                if (ie == null)
                {
                    yield break;
                }

                ie.Reset();
                while (true)
                {
                    if (!m_host.Fsm.gameObject.activeInHierarchy
                        || !m_host.Fsm.enabled)
                    {
                        yield return null;
                        continue;
                    }
                    var r = ie.MoveNext();
                    if (!r) break;
                    yield return ie.Current;
                }
                yield break;
            }
        }

    }
}
