using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Modding;
using HKTool.DebugTools;
using HKTool.FSM;
using UnityEngine;

namespace HKTool
{
    public abstract class ModBase : Mod
    {
        protected virtual bool ShowDebugView => true;
        public ModBase(string name = null) : base(name)
        {
            if(this is IDebugViewBase @base && ShowDebugView)
            {
                DebugView.debugViews.Add(@base);
            }
            foreach(var v in GetType().GetRuntimeMethods())
            {
                if (v.ReturnType != typeof(void) || !v.IsStatic
                    || v.GetParameters().Length != 1 || v.GetParameters().FirstOrDefault()?.ParameterType != typeof(FSMQuene)) continue;
                var d = (FsmCatch_Handler)v.CreateDelegate(typeof(FsmCatch_Handler));
                foreach(var attr in v.GetCustomAttributes<FsmPatchAttribute>()){
                    FsmManager.handlers.Add((attr, d));
                }
            }
        }
        public virtual void GetModName() => GetName();

        public virtual void OnDebugDraw()
        {
            GUILayout.Label("Empty DebugView");
        }
    }
}
