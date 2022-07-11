
namespace HKTool.FSM;

public class InvokeAction : FsmStateAction
{
    public Action<FsmStateAction> action;
    public InvokeAction(Action<FsmStateAction> action)
    {
        this.action = action;
    }
    public InvokeAction(Action action) : this((_) => action())
    {

    }
    public static implicit operator InvokeAction(Action action) => new(action);
    public static implicit operator InvokeAction(Action<FsmStateAction> action) => new(action);
    public override void OnEnter()
    {
        action(this);
        Finish();
    }
}
