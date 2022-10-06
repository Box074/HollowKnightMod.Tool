
namespace HKTool.FSM;
public delegate FsmStateAction ForEachFsmStateActionDelegate<T>(T action) where T : FsmStateAction;
public delegate FsmTransition ForEachFsmTransitionDelegate(FsmTransition transition);

public static class FSMHelper
{
    public static PlayMakerFSM CopyTo(this PlayMakerFSM src, GameObject dst)
    {
        var template = new FsmTemplate()
        {
            fsm = src.Fsm
        };
        var pm = dst.AddComponent<PlayMakerFSM>();
        pm.SetFsmTemplate(template);
        pm.Fsm.Name = src.Fsm.Name;
        return pm;
    }
    public static PropertyInfo GetVariableArray(VariableType type)
    {
        string name = type switch
        {
            _ => type.ToString() + "Variables"
        };
        return typeof(FsmVariables).GetProperty(name) ?? throw new InvalidOperationException();
    }

    public static Fsm[] FindFsms(string name)
    {
        return PlayMakerFSM.FsmList.Select(x => x.Fsm).Where(x => x.Name == name).ToArray();
    }
    public static Fsm? FindFsm(string name)
    {
        return PlayMakerFSM.FsmList.FirstOrDefault(x => x.Fsm.Name == name)?.Fsm;
    }
    public static FsmState? FindState(this GameObject go, string name)
    {
        foreach (var v in go.GetComponents<PlayMakerFSM>())
        {
            var s = v.Fsm.GetState(name);
            if (s != null) return s;
        }
        return null;
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
        var arr = new FsmStateAction[state.Actions.Length + 1];
        var offset = 0;
        for(int i = 0; i < arr.Length ; i++)
        {
            if(i == index)
            {
                arr[i] = action;
                offset = -1;
                continue;
            }
            arr[i] = state.Actions[i + offset];
        }
        state.Actions = arr;
        action.Init(state);
        return action;
    }
    public static T AppendFsmStateAction<T>(this FsmState state, T action) where T : FsmStateAction
    {
        var arr = new FsmStateAction[state.Actions.Length + 1];
        Array.Copy(state.Actions, arr, state.Actions.Length);
        arr[arr.Length - 1] = action;
        state.Actions = arr;
        action.Init(state);
        return action;
    }
    public static void RemoveAllFsmStateActions<T>(this FsmState state) where T : FsmStateAction
    {
        state.Actions = state.Actions.Where(x => !(x is T)).ToArray();
    }
    public static void ForEachFsmStateAction<T>(this FsmState state, ForEachFsmStateActionDelegate<T> fun) where T : FsmStateAction
    {
        var l = new List<FsmStateAction>(state.Actions.Length);
        foreach (var v in state.Actions)
        {
            var t = v;
            if (t is T o)
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
        return new FSMPatch(fsm);
    }
    public static FsmState InsertFSMState(this Fsm fsm, FsmState state)
    {
        fsm.States = fsm.States.Append(state).ToArray();
        return state;
    }
    public static FsmState? GetFSMState(this GameObject go, string state, string? fsm = null)
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
        return new InvokeAction(action);
    }
}

