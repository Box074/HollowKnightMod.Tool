using HKTool.Utils;
using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.FSM.CSFsmEx.Compiler
{
    internal class CSFsmBuildContext
    {
        public CSFsmHost Host { get; set; } = null!;
        public Fsm Fsm { get; set; } = null!;
        public GameObject GameObject => Fsm.GameObject;

        public GameObject? FindChild(string[] names)
        {
            return GameObject.FindChildWithPath(names);
        }
        public CSFsmStateActionWalker<T> GetActionWalker<T>() where T : FsmStateAction
        {
            return new(Host.CurrentStateMetadata.originalActions.GetEnumerator(), typeof(T));
        }
        public static T? GetNextAction<T>(CSFsmStateActionWalker<T> walker) where T : FsmStateAction => walker.Next();
        public static T? GetNextActionOrNew<T>(CSFsmStateActionWalker<T> walker) where T : FsmStateAction, new() 
            => walker.Next() ?? new();
        public T[] GetAllActions<T>() where T : FsmStateAction
        {
            return Host.CurrentStateMetadata.originalActions.OfType<T>().ToArray();
        }
        
        public void InitState(string name)
        {
            Host.InitState(name);
        }
        public void RegisterState(IEnumerator template, bool hasInit)
        {
            Host.BuildState(template, hasInit);
        }
        public FsmEvent RegisterFsmTransition(string name, string target)
        {
            var ev = FsmEvent.GetFsmEvent(name);
            Host.CurrentStateMetadata.transitions.Add((ev, target));
            return ev;
        }
        public static T[] MergeFsmVar<T>(T[] other, T[] variables, int count)
        {
            Array.Resize(ref variables, variables.Length + count);
            Array.Copy(other, 0, variables, variables.Length - other.Length, count);
            return variables;
        }
        
    }
    internal class CSFsmStateActionWalker<T> where T : FsmStateAction
    {
        public IEnumerator<FsmStateAction> Actions { get; }
        public Type ActionType { get; }
        public CSFsmStateActionWalker(IEnumerator<FsmStateAction> actions, Type actionType)
        {
            Actions = actions;            ActionType = actionType;
        }
        public T? Next()
        {
            while(Actions.MoveNext())
            {
                if(Actions.Current?.GetType() == ActionType)
                {
                    return (T)Actions.Current;
                }
            }
            return null;
        }
    }
}
