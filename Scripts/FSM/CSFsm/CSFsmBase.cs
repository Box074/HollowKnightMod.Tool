
namespace HKTool.FSM.CSFsm;

public abstract class CSFsmBase
{
    [AttributeUsage(AttributeTargets.Method)]
    protected class FsmStateAttribute : Attribute
    {

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
    public PlayMakerFSM? FsmComponent { get; private set; }
    private StateInfo? currentState;
    private FsmStateAction[]? _origActions;
    private FsmState? _origState;
    protected FsmState? OriginalState
    {
        get
        {
            CheckInit();
            if(_origState == null)
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
                _origActions = OriginalState?.Actions ?? Array.Empty<FsmStateAction>();
            }
            return _origActions;
        }
    }
    [Obsolete]
    protected FsmStateAction[] OrigActions
    {
        get
        {
            CheckInit();
            if (_origActions == null)
            {
                _origActions = OriginalState?.Actions ?? Array.Empty<FsmStateAction>();
            }
            return _origActions;
        }
    }
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
    protected void CopyEvent(string eventName)
    {
        CheckInit();
        DefineEvent(eventName, 
            OriginalState?.Transitions?.First(x => x.EventName == eventName).ToState 
            ?? throw new InvalidOperationException());
    }
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
        if(count > actions.Length) count = actions.Length;
        var state = current.State;
        var act = state.Actions;
        var beg = act.Length;
        Array.Resize(ref act, act.Length + count);
        state.Actions = act;
        for(int i = 0; i < count ; i++)
        {
            var action = actions[i + index];
            if(action is null)
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
            if (!v.IsDefined(typeof(FsmStateAttribute))) continue;
            BuildState(v, fsm, states);
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
            var varName = string.IsNullOrEmpty(d.varName) ? v.Name : d.varName;
            var val = (NamedVariable)v.FastGet(this)!;
            if (val is null) continue;
            var p = FSMHelper.GetVariableArray(val.VariableType);
            var origArr = (NamedVariable[])p.FastGet(fsm.Variables)!;
            var orig = origArr.FirstOrDefault(x => x.Name == varName);
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
    private FsmState BuildState(MethodInfo m, Fsm fsm, List<FsmState> states)
    {
        FsmState state;
        try
        {
            currentState = new();
            _origActions = null;
            currentState.name = m.Name;
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
            if(state == null)
            {
                state = new(fsm);
                states.Add(state);
            }
            state.Transitions = trans;
            state.Name = currentState.name;

            state.Actions = new FsmStateAction[]
            {
                new CSFsmAction()
                {
                    fsm = this,
                    template = ie,
                    ActionMethod = m.DeclaringType.FullName + "::" + m.Name
                }
            };
            state.IgnoreLoadActionData();
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
        [NonSerialized]
        public CSFsmBase? fsm;
        [NonSerialized]
        public IEnumerator? template;
        [NonSerialized]
        public IEnumerator? currentAction;
        [NonSerialized]
        public CoroutineInfo? currentEx;
        [NonSerialized]
        public List<FsmStateAction> actions = new();
        public string? ActionMethod;
        public override void OnEnter()
        {
            fsm!.current = this;
            currentEx = ExecuteAction().StartCoroutine();
            currentEx.onFinished += (_) =>
            {
                if (currentAction is null) return;
                Fsm.Event(FsmEvent.Finished);
            };
        }
        private IEnumerator ExecuteAction()
        {
            currentAction = template!.MemberwiseClone();
            while (true)
            {
                if (!(fsm?.FsmComponent?.gameObject?.activeInHierarchy ?? false)
                    || !(fsm?.FsmComponent?.enabled ?? false))
                {
                    yield return null;
                    continue;
                }
                var r = currentAction.MoveNext();
                if (!r) break;
                var result = currentAction.Current;
                if (result is string eventName)
                {
                    Fsm.Event(FsmEvent.GetFsmEvent(eventName));
                    yield return null;
                    continue;
                }
                yield return result;
            }
            yield break;
        }
        public override void OnExit()
        {
            currentAction = null;
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
    public static T Apply(PlayMakerFSM pm)
    {
        var inst = new T();
        inst.BindPlayMakerFSM(pm);
        return inst;
    }
    public static PlayMakerFSM Attach(GameObject go, string startState = "")
    {
        var pm = go.AddComponent<PlayMakerFSM>();
        Apply(pm);
        pm.Fsm.StartState = startState;
        pm.Fsm.Name = typeof(T).Name;
        pm.SetState(startState);
        return pm;
    }
}
