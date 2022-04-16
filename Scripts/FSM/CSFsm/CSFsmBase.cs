
namespace HKTool.FSM.CSFsm;

[ModuleDefine("CSFsm", "1.0")]
public abstract class CSFsmBase
{
    [AttributeUsage(AttributeTargets.Method)]
    protected class FsmStateAttribute : Attribute
    {

    }
    private static MethodInfo M_clone = typeof(object).GetMethod("MemberwiseClone", HReflectionHelper.All);
    private class StateInfo
    {
        public Fsm? fsm;
        public string name = "";
        public List<(string eventName, string toState)> events = new();
    }
    private Dictionary<MethodInfo, StateInfo> stateCache = new();
    protected static object StartActionContent { get; } = new object();
    private CSFsmAction? current = null;
    public PlayMakerFSM? FsmComponent { get; private set; }
    private StateInfo? currentState;
    private FsmStateAction[]? _origActions;
    protected FsmStateAction[] OrigActions
    {
        get
        {
            if(_origActions == null)
            {
                _origActions = currentState?.fsm?.GetState(currentState?.name)?.Actions ?? Array.Empty<FsmStateAction>();
            }
            return _origActions;
        }
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
    protected void InvokeAction(FsmStateAction action)
    {
        if(current is null) return;
        var state = current.State;
        var actions = state.Actions;
        Array.Resize(ref actions, actions.Length + 1);
        state.Actions = actions;
        state.Actions[actions.Length - 1] = action;
        
        action.Active = true;
        action.Finished = false;
        action.Init(state);
        action.Entered = true;
        action.OnEnter();
        current.actions.Add(action);
        if(!action.Finished)
        {
            state.ActiveActions.Add(action);
        }
    }
    protected IEnumerator InvokeActionAndWait(FsmStateAction action)
    {
        InvokeAction(action);
        while(!action.Finished) yield return null;
    }
    protected T CloneAndInvokeAction<T>(T action) where T : FsmStateAction
    {
        var a = (T)M_clone.FastInvoke(action)!;
        InvokeAction(a);
        return a;
    }
    protected IEnumerator CloneAndInvokeActionAndWait<T>(T action, out T outAction) where T : FsmStateAction
    {
        var a = (T)M_clone.FastInvoke(action)!;
        outAction = a;
        return InvokeActionAndWait(a);
    }
    protected void ForceStopAction(FsmStateAction action, bool callOnExit = true)
    {
        if(action.Finished) return;
        if(current is null) return;
        if(current.actions.Remove(action))
        {
            for(int i = 0 ; i < current.State.Actions.Length ; i++)
            {
                if(ReferenceEquals(current.State.Actions[i], action))
                {
                    current.State.Actions[i] = EmptyAction.action;
                    break;
                }
            }
            for(int i = 0 ; i < current.State.ActiveActions.Count ; i++)
            {
                if(ReferenceEquals(current.State.ActiveActions[i], action))
                {
                    current.State.ActiveActions[i] = EmptyAction.action;
                    break;
                }
            }
        }
        if(callOnExit)
        {
            action.Init(current.State);
            action.OnExit();
        }
    }
    protected IEnumerator WaitForActionFinish(FsmStateAction action) 
    {
        while(!action.Finished) yield return null;
    }
    protected IEnumerator WaitForAllActions()
    {
        while(current?.actions?.Any(x => !x.Finished) ?? false) yield return null;
    }
    private void BindFsm(Fsm fsm)
    {
        var methods = GetType().GetMethods(HReflectionHelper.All);
        List<FsmState> states = new(fsm.States);
        foreach(var v in methods)
        {
            if(!v.IsDefined(typeof(FsmStateAttribute))) continue;
            var state = BuildState(v, fsm);
            states.Add(state);
        }
        fsm.States = states.ToArray();
        foreach(var v in states)
        {
            foreach(var e in v.Transitions)
            {
                e.ToFsmState = fsm.GetState(e.ToState);
            }
        }
    }
    private FsmState BuildState(MethodInfo m, Fsm fsm)
    {
        var state = new FsmState(fsm);
        try
        {
            currentState = new();
            _origActions = null;
            currentState.name = m.Name;
            var ie = (IEnumerator)m.FastInvoke(this)!;
            if (!ie.MoveNext()) throw new InvalidOperationException();
            if(ie.Current != StartActionContent) throw new InvalidOperationException();
            var trans = new FsmTransition[currentState.events.Count];
            for(int i = 0 ; i < currentState.events.Count ; i++)
            {
                var e = currentState.events[i];
                trans[i] = new()
                {
                    FsmEvent = FsmEvent.GetFsmEvent(e.eventName),
                    ToState = e.toState
                };
            }
            state.Transitions = trans;
            state.Name = currentState.name;

            state.Actions = new FsmStateAction[]
            {
                new CSFsmAction()
                {
                    fsm = this,
                    template = ie
                }
            };

        }
        finally
        {
            currentState = null;
            _origActions = null;
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
        public CSFsmBase? fsm;
        public IEnumerator? template;
        public IEnumerator? currentAction;
        public CoroutineInfo? currentEx;
        public List<FsmStateAction> actions = new();
        public override void OnEnter()
        {
            fsm!.current = this;
            currentEx = ExecuteAction().StartCoroutine();
            currentEx.onFinished += (_) =>
            {
                if(currentAction is null) return;
                Fsm.Event(FsmEvent.Finished);
            };
        }
        private IEnumerator ExecuteAction()
        {
            currentAction = (IEnumerator)M_clone.FastInvoke(template!)!;
            while(true)
            {
                if(!(fsm?.FsmComponent?.gameObject?.activeInHierarchy ?? false)
                    || !(fsm?.FsmComponent?.enabled ?? false))
                    {
                        yield return null;
                        continue;
                    }
                var r = currentAction.MoveNext();
                if(!r) break;
                var result = currentAction.Current;
                if(result is string eventName)
                {
                    Fsm.ProcessEvent(FsmEvent.GetFsmEvent(eventName));
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
            foreach(var v in actions)
            {
                v.Init(State);
                v.OnExit();
            }
            actions.Clear();
        }

    }
}

public abstract class CSFsm<T> : CSFsmBase where T : CSFsm<T> , new()
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
        pm.SetState(startState);
        return pm;
    }
}
