
namespace HKTool.FSM;
public class FSMPatch : IPatch
{
    internal FSMPatch(Fsm fsm)
    {
        TargetFSM = fsm;
        States = fsm.States.ToList();
        EditState(fsm.StartState);
        Modding.Logger.Log($"State Count:  {States.Count} Name: { fsm.GameObject.name }");
    }
    public Fsm TargetFSM { get; private set; }
    public FsmState? CurrentState { get; private set; }
    public List<FsmState> States { get; private set; } = new();
    public List<FsmTransition> Transitions { get; private set; } = new();
    public List<FsmStateAction> Actions { get; private set; } = new();
    public List<(FsmTransition, string)> DelayBindTransitions { get; private set; } = new List<(FsmTransition, string)>();
    public FsmStateAction? LastOperationAction { get; private set; }
    private bool isClose = false;
    ~FSMPatch()
    {
        if (!isClose)
        {
            EndFSMEdit();
        }
    }

    private void TestIsClose()
    {
        if (isClose) throw new InvalidOperationException();
    }
    public FSMPatch FlushState()
    {
        TestIsClose();
        if (CurrentState != null)
        {
            CurrentState.Actions = Actions.ToArray();
            CurrentState.Transitions = Transitions.ToArray();
            CurrentState.SaveActions();
        }
        return this;
    }

    public FSMPatch FlushFsm()
    {
        TestIsClose();
        foreach (var v in DelayBindTransitions)
        {
            var s = FindState(v.Item2);
            if (s == null) throw new InvalidOperationException();
            v.Item1.ToState = s.Name;
            v.Item1.ToFsmState = s;
        }
        DelayBindTransitions.Clear();
        FlushState();
        TargetFSM.States = States.ToArray();
        return this;
    }
    public FSMPatch DelayBindTransition(FsmTransition transition, string stateName)
    {
        if (transition == null) throw new ArgumentNullException(nameof(transition));
        if (string.IsNullOrEmpty(stateName)) throw new ArgumentException();
        DelayBindTransitions.Add((transition, stateName));
        return this;
    }
    public FSMPatch DelayBindTransition(string eventName, string stateName)
    {
        FindTransition(eventName, t => DelayBindTransition(t, stateName));
        return this;
    }
    public void EndFSMEdit()
    {
        if (isClose) return;
        FlushFsm();
        isClose = true;
        GC.SuppressFinalize(this);
    }
    public FSMPatch EditState(string name)
    {
        TestIsClose();
        if (CurrentState != null)
        {
            FlushState();
        }
        CurrentState = States.FirstOrDefault(x => x.Name == name);
        if (CurrentState == null) throw new InvalidOperationException();
        if (Transitions == null) Transitions = CurrentState.Transitions.ToList();
        else
        {
            Transitions.Clear();
            Transitions.AddRange(CurrentState.Transitions);
        }
        if (Actions == null) Actions = CurrentState.Actions.ToList();
        else
        {
            Actions.Clear();
            Actions.AddRange(CurrentState.Actions);
        }
        return this;
    }
    public FSMPatch InsertAction(FsmStateAction action, int index)
    {
        TestIsClose();
        LastOperationAction = action;
        Actions.Insert(index, action);
        return this;
    }
    public FSMPatch AppendAction(FsmStateAction action)
    {
        TestIsClose();
        LastOperationAction = action;
        Actions.Add(action);
        return this;
    }
    public FSMPatch FindTransition(string name, Action<FsmTransition> func)
    {
        TestIsClose();
        func(Transitions.FirstOrDefault(x => x.EventName == name));
        return this;
    }
    public FSMPatch FindTransition(string name, Action<FsmTransition, FSMPatch> func)
    {
        TestIsClose();
        func(Transitions.FirstOrDefault(x => x.EventName == name), this);
        return this;
    }
    public FsmState FindState(string name)
    {
        TestIsClose();
        return States.FirstOrDefault(x => x.Name == name);
    }
    public FSMPatch AppendTransition(string name, string toState)
    {
        TestIsClose();
        FsmTransition ft = new FsmTransition();
        Transitions.Add(ft);
        ft.FsmEvent = FsmEvent.GetFsmEvent(name);
        ft.ToState = toState;
        ft.ToFsmState = FindState(toState);
        return this;
    }
    public FSMPatch ChangeTransition(string name, string toState)
    {
        TestIsClose();
        FindTransition(name, t =>
        {
            t.ToFsmState = FindState(toState);
            t.ToState = toState;
        });
        return this;
    }
    public FSMPatch RemoveTransition(string name)
    {
        TestIsClose();
        Transitions.RemoveAll(x => x.EventName == name);
        return this;
    }

    public FSMPatch FindAction<T>(Action<T> func, int index) where T : FsmStateAction
    {
        TestIsClose();
        var action = Actions.OfType<T>().ElementAtOrDefault(index);
        LastOperationAction = action;
        func(action);
        return this;
    }

    public FSMPatch FindAction<T>(Action<T> func) where T : FsmStateAction => FindAction(func, 0);
    public FSMPatch EditLastAction<T>(Action<T> func) where T : FsmStateAction
    {
        TestIsClose();
        if (LastOperationAction == null || !(LastOperationAction is T)) throw new InvalidOperationException();
        func((T)LastOperationAction);
        return this;
    }
    public FSMPatch FindAction<T>(Action<T, FSMPatch> func, int index) where T : FsmStateAction
    {
        TestIsClose();
        var action = Actions.OfType<T>().ElementAtOrDefault(index);
        LastOperationAction = action;
        func(action, this);
        return this;
    }

    public FSMPatch FindAction<T>(Action<T, FSMPatch> func) where T : FsmStateAction => FindAction(func, 0);
    public FSMPatch EditLastAction<T>(Action<T, FSMPatch> func) where T : FsmStateAction
    {
        TestIsClose();
        if (LastOperationAction == null || !(LastOperationAction is T)) throw new InvalidOperationException();
        func((T)LastOperationAction, this);
        return this;
    }
    public FSMPatch EditLastAction(Action<FsmStateAction, FSMPatch> func) => EditLastAction<FsmStateAction>(func);
    public FSMPatch ForEachFsmStateTransitions(Func<FsmTransition, FsmTransition> fun)
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
    public FSMPatch ForEachFsmStateTransitions(Func<FsmTransition, FSMPatch, FsmTransition> fun)
    {
        TestIsClose();
        for (int i = 0; i < Transitions.Count; i++)
        {
            var v = fun(Transitions[i], this);
            Transitions[i] = v;
        }
        Actions.RemoveAll(x => x is null);
        return this;
    }
    public FSMPatch ForEachFsmStateActions<T>(Func<T, FsmStateAction> fun) where T : FsmStateAction
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
    public FSMPatch ForEachFsmStateActions<T>(Func<T, FSMPatch, FsmStateAction> fun) where T : FsmStateAction
    {
        TestIsClose();
        for (int i = 0; i < Actions.Count; i++)
        {
            var v = Actions[i] as T;
            if (v != null)
            {
                var r = fun(v, this);
                Actions[i] = r;
            }
        }
        Actions.RemoveAll(x => x is null);
        return this;
    }

    public FSMPatch AddState(string name)
    {
        TestIsClose();
        var s = new FsmState(TargetFSM)
        {
            Name = name
        };
        States.Add(s);
        return this;
    }
    public FSMPatch AddStateAndEdit(string name)
    {
        var s = new FsmState(TargetFSM)
        {
            Name = name
        };
        States.Add(s);
        EditState(name);
        return this;
    }

    void IDisposable.Dispose()
    {
        EndFSMEdit();
    }

}
