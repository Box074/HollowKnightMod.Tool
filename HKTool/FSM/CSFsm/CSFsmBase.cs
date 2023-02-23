
namespace HKTool.FSM.CSFsm;

[Serializable] //FIXME
public abstract class CSFsmBase : MonoBehaviour
{
    [AttributeUsage(AttributeTargets.Method)]
    protected class FsmStateAttribute : Attribute
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
    protected class FsmVarAttribute : Attribute
    {
        public string varName;
        public FsmVarAttribute([CallerMemberName] string name = "")
        {
            varName = name;
        }
    }

    private class StateInfo
    {
        public Fsm? fsm = null;
        public string name = "";
        public List<(string eventName, string toState)> events = new();
        public List<string> globalEvents = new();
    }


    private Dictionary<MethodInfo, StateInfo> stateCache = new();
    protected static object StartActionContent { get; } = new object();
    private CSFsmAction? current = null;
    [field: SerializeField]
    [field: SerializeReference]
    public PlayMakerFSM FsmComponent { get; private set; } = null!;
    private StateInfo? currentState;
    private FsmStateAction[]? _origActions;
    private FsmState? _origState;
    protected FsmState? OriginalState
    {
        get
        {
            CheckInit();
            if (_origState == null)
            {
                _origState = currentState?.fsm?.GetState(currentState?.name);
            }
            return _origState;
        }
    }
    protected FsmStateAction[] OriginalActions
    {
        get
        {
            CheckInit();
            if (_origActions == null)
            {
                var state = OriginalState;
                if(state == null) return _origActions = Array.Empty<FsmStateAction>();
                _origActions = state.Actions;
            }
            return _origActions;
        }
    }
    [Obsolete]
    protected FsmStateAction[] OrigActions => OriginalActions;
    protected T GetOriginalAction<T>(int id) where T : FsmStateAction
    {
        return (T)OriginalActions[id];
    }
    [Obsolete]
    protected T GetOrigAction<T>(int id) where T : FsmStateAction
    {
        return (T)OriginalActions[id];
    }
    internal void BindPlayMakerFSM(PlayMakerFSM pm)
    {
        FsmComponent = pm;
        OnBindPlayMakerFSM(pm);
        BindFsm(pm.Fsm);
        OnAfterBindPlayMakerFSM();
    }
    private void CheckInit()
    {
        if (currentState is null) throw new InvalidOperationException();
    }
    protected virtual void OnAfterBindPlayMakerFSM()
    {

    }
    protected virtual void OnBindPlayMakerFSM(PlayMakerFSM pm)
    {

    }
    protected void SetName([CallerMemberName] string name = "")
    {
        CheckInit();
        currentState!.name = name;
    }
    protected void DefineEvent(string eventName, string toState)
    {
        CheckInit();
        currentState!.events.Add((eventName, toState));
    }
    protected void DefineEvent(FsmEvent eventName, string toState)
    {
        DefineEvent(eventName.Name, toState);
    }
    protected void CopyEvent(string eventName)
    {
        CheckInit();
        DefineEvent(eventName,
            OriginalState?.Transitions?.First(x => x.EventName == eventName).ToState
            ?? throw new InvalidOperationException());
    }
    protected void CopyEvent(FsmEvent eventName) => CopyEvent(eventName.Name);
    protected void DefineGlobalEvent(string eventName)
    {
        CheckInit();
        currentState!.globalEvents.Add(eventName);
    }
    protected void InvokeAction(FsmStateAction action)
    {
        if (current is null) return;
        var state = current.State;
        var actions = state.Actions;
        Array.Resize(ref actions, actions.Length + 1);
        state.Actions = actions;
        state.Actions[actions.Length - 1] = action;
        InvokePMAction(action);
    }
    protected void InvokeActions(FsmStateAction[] actions, int index, int count)
    {
        if (current is null) return;
        if (count > actions.Length) count = actions.Length;
        var state = current.State;
        var act = state.Actions;
        var beg = act.Length;
        Array.Resize(ref act, act.Length + count);
        state.Actions = act;
        for (int i = 0; i < count; i++)
        {
            var action = actions[i + index];
            if (action is null)
            {
                act[beg + i] = EmptyAction.action;
                continue;
            }
            act[beg + i] = action;
            InvokePMAction(action);
        }
    }
    protected void InvokeActions(FsmStateAction[] actions, int index = 0)
    {
        InvokeActions(actions, index, actions.Length - index);
    }
    private void InvokePMAction(FsmStateAction action)
    {
        if (current is null) return;
        var state = current.State;
        action.Active = true;
        action.Finished = false;
        action.Init(state);
        action.Entered = true;
        action.OnEnter();
        current.actions.Add(action);
        if (!action.Finished)
        {
            state.ActiveActions.Add(action);
        }
    }
    protected IEnumerator InvokeActionAndWait(FsmStateAction action)
    {
        InvokeAction(action);
        while (!action.Finished) yield return null;
    }
    protected T CloneAndInvokeAction<T>(T action) where T : FsmStateAction
    {
        var a = action.MemberwiseClone();
        InvokeAction(a);
        return a;
    }
    protected IEnumerator CloneAndInvokeActionAndWait<T>(T action, out T outAction) where T : FsmStateAction
    {
        var a = action.MemberwiseClone();
        outAction = a;
        return InvokeActionAndWait(a);
    }
    protected void ForceStopAction(FsmStateAction action, bool callOnExit = true)
    {
        if (action.Finished) return;
        if (current is null) return;
        if (current.actions.Remove(action))
        {
            for (int i = 0; i < current.State.Actions.Length; i++)
            {
                if (ReferenceEquals(current.State.Actions[i], action))
                {
                    current.State.Actions[i] = EmptyAction.action;
                    break;
                }
            }
            for (int i = 0; i < current.State.ActiveActions.Count; i++)
            {
                if (ReferenceEquals(current.State.ActiveActions[i], action))
                {
                    current.State.ActiveActions[i] = EmptyAction.action;
                    break;
                }
            }
        }
        if (callOnExit)
        {
            action.Init(current.State);
            action.OnExit();
        }
    }
    protected IEnumerator WaitForActionFinish(FsmStateAction action)
    {
        while (!action.Finished) yield return null;
    }
    protected IEnumerator WaitForAllActions()
    {
        while (current?.actions?.Any(x => !x.Finished) ?? false) yield return null;
    }
    protected IEnumerator WaitForAllActions(FsmStateAction[] actions)
    {
        while (actions.Any(x => !x.Finished)) yield return null;
    }
    private void BindFsm(Fsm fsm)
    {
        BindVar(fsm);
        var methods = GetType().GetMethods(HReflectionHelper.All);
        List<FsmState> states = new(fsm.States);
        foreach (var v in methods)
        {
            var attr = v.GetCustomAttribute<FsmStateAttribute>();
            if (attr == null) continue;
            BuildState(v, fsm, states, string.IsNullOrEmpty(attr.name) ? v.Name : attr.name);
        }
        fsm.States = states.ToArray();
        foreach (var v in states)
        {
            foreach (var e in v.Transitions)
            {
                e.ToFsmState = fsm.GetState(e.ToState);
            }
        }
    }

    private void BindVar(Fsm fsm)
    {
        foreach (var v in GetType().GetFields(HReflectionHelper.All))
        {
            if (!v.FieldType.IsSubclassOf(typeof(NamedVariable))) continue;
            var d = v.GetCustomAttribute<FsmVarAttribute>();
            if (d is null) continue;
            var varName = string.IsNullOrEmpty(d.varName) ? v.Name : d.varName;
            var val = (NamedVariable)v.FastGet(this)!;
            if (val is null) continue;
            var p = FSMHelper.GetVariableArray(val.VariableType);
            var origArr = (NamedVariable[])p.FastGet(fsm.Variables)!;
            var orig = origArr.FirstOrDefault(x => x.Name == varName) ?? ((NamedVariable[])p.FastGet(FsmVariables.GlobalVariables)!).FirstOrDefault(x => x.Name == varName);
            if (orig is null)
            {
                var newArr = (NamedVariable[])Array.CreateInstance(p.PropertyType.GetElementType(), origArr.Length + 1);
                origArr.CopyTo(newArr, 0);
                newArr[origArr.Length] = val;
                val.Name = varName;
                p.FastSet(fsm.Variables, newArr);
            }
            else
            {
                v.FastSet(this, orig);
            }
        }
    }
    private FsmState BuildState(MethodInfo m, Fsm fsm, List<FsmState> states, string name)
    {
        FsmState state;
        try
        {
            currentState = new();
            _origActions = null;
            currentState.name = name;
            currentState.fsm = fsm;
            var ie = (IEnumerator)m.FastInvoke(this)!;
            if (!ie.MoveNext()) throw new InvalidOperationException();
            if (ie.Current != StartActionContent) throw new InvalidOperationException();
            var trans = new FsmTransition[currentState.events.Count];
            for (int i = 0; i < currentState.events.Count; i++)
            {
                var e = currentState.events[i];
                trans[i] = new()
                {
                    FsmEvent = FsmEvent.GetFsmEvent(e.eventName),
                    ToState = e.toState
                };
            }

            state = OriginalState!;

            if (state == null)
            {
                state = new(fsm);
                states.Add(state);
            }

            state.Transitions = trans;
            state.Name = currentState.name;

            state.Actions = new FsmStateAction[]
            {
                new CSFsmAction(ie)
                {
                    fsm = this,
                    ActionMethod = m.DeclaringType.FullName + "::" + m.Name
                }
            };
            if (currentState.globalEvents.Count != 0)
            {
                var gts = new List<FsmTransition>(fsm.GlobalTransitions);
                foreach (var v in currentState.globalEvents)
                {
                    gts.Add(new()
                    {
                        FsmEvent = FsmEvent.GetFsmEvent(v),
                        ToState = currentState.name,
                        ToFsmState = state
                    });
                }
                fsm.GlobalTransitions = gts.ToArray();
            }
        }
        finally
        {
            currentState = null;
            _origActions = null;
            _origState = null;
        }
        return state;
    }
    private class EmptyAction : FsmStateAction
    {
        public static EmptyAction action = new();
        public override void OnEnter()
        {
            Finish();
        }
    }
    private class CSFsmAction : FsmStateAction
    {
        public CSFsmAction(IEnumerator template)
        {
            _current = template.AsResettable();
        }
        [NonSerialized]
        public CSFsmBase fsm = null!;
        [NonSerialized]
        public ResettableEnumerator _current;
        [NonSerialized]
        public CoroutineInfo? currentEx;
        [NonSerialized]
        public List<FsmStateAction> actions = new();
        public string? ActionMethod;
        public override void OnEnter()
        {
            fsm!.current = this;
            currentEx = ExecuteAction().CreateCoroutine();
            currentEx.onFinished += (_) =>
            {
                if(currentEx is null) return;
                HKToolMod.logger.LogFine($"Finished State: {ActionMethod}");
                Fsm.Event(FsmEvent.Finished);
            };
            currentEx.onException += (_, e) =>
            {
                HKToolMod.logger.LogError(e);
            };
            currentEx.customResult = (ref object? result) =>
            {
                if (result is string eventName)
                {
                    Fsm.Event(FsmEvent.GetFsmEvent(eventName));
                    result = null;
                    return true;
                }
                else if(result is FsmEvent eve)
                {
                    Fsm.Event(eve);
                    result = null;
                    return true;
                }
                else if (result is FsmStateAction action)
                {
                    fsm.InvokeAction(action);
                    return false;
                }
                else if (result is FsmStateAction[] actions)
                {
                    fsm.InvokeActions(actions);
                    return false;
                }
                else if (result is ValueTuple<float, string> (var delay,var ev))
                {
                    fsm.FsmComponent.Fsm.DelayedEvents.Add(new(fsm.FsmComponent.Fsm, FsmEvent.GetFsmEvent(ev), delay));
                    fsm.FsmComponent.Fsm.UpdateDelayedEvents();
                }
                return true;
            };
            currentEx.Start();
        }
        private IEnumerator ExecuteAction()
        {
            _current.Reset();
            while (true)
            {
                if (!(fsm?.FsmComponent?.gameObject?.activeInHierarchy ?? false)
                    || !(fsm?.FsmComponent?.enabled ?? false))
                {
                    yield return null;
                    continue;
                }
                var r = _current.MoveNext();
                if (!r) break;
                yield return _current.Current;
            }
            yield break;
        }
        public override void OnExit()
        {
            currentEx?.Stop();
            currentEx = null;
            State.Actions = new FsmStateAction[]
            {
                this
            };
            foreach (var v in actions)
            {
                v.Init(State);
                v.OnExit();
            }
            actions.Clear();
        }

    }
}

public abstract class CSFsm<T> : CSFsmBase where T : CSFsm<T>, new()
{
    public virtual string InitState => "";
    public static T Apply(PlayMakerFSM pm)
    {
        var inst = pm.gameObject.AddComponent<T>();
        inst.BindPlayMakerFSM(pm);
        return inst;
    }
    public static PlayMakerFSM Attach(GameObject go, string startState = "")
    {
        return Attach(go, out _, startState);
    }
    public static PlayMakerFSM Attach(GameObject go, out T fsm, string startState = "")
    {
        
        var pm = go.AddComponent<PlayMakerFSM>();

        var t = Apply(pm);
        startState = string.IsNullOrEmpty(startState) ? t.InitState : startState;
        pm.Fsm.StartState = startState;
        pm.Fsm.Name = typeof(T).Name;
        pm.SetState(startState);
        fsm = t;
        return pm;
    }
}
