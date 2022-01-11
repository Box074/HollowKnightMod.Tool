using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Modding;
using HKTool.DebugTools;
using HKTool.FSM;
using UnityEngine;
using System.Text.RegularExpressions;

namespace HKTool
{
    public abstract class ModBase : Mod
    {
        private static FsmFilter CreateFilter(FsmPatcherAttribute attr)
        {
			if(attr.useRegex)
			{
            return new FsmNameFilterRegex(string.IsNullOrEmpty(attr.sceneName) ? null : new Regex(attr.sceneName),
            string.IsNullOrEmpty(attr.objName) ? null : new Regex(attr.objName),
            string.IsNullOrEmpty(attr.fsmName) ? null : new Regex(attr.fsmName));
			}
			else
			{
				return new FsmNameFilter(attr.sceneName, attr.objName, attr.fsmName);
			}
        }
        protected virtual bool ShowDebugView => true;
        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }
        public virtual I18n I18n => _i18n.Value;
        public ModBase(string name = null) : base(name)
        {
            if (this is IDebugViewBase @base && ShowDebugView)
            {
                DebugView.debugViews.Add(@base);
            }
            foreach (var v in GetType().GetRuntimeMethods())
            {
                if (v.ReturnType != typeof(void) || !v.IsStatic
                    || v.GetParameters().Length != 1 || v.GetParameters().FirstOrDefault()?.ParameterType != typeof(FSMPatch)) continue;

                var d = (WatchHandler<FSMPatch>)v.CreateDelegate(typeof(WatchHandler<FSMPatch>));
                foreach (var attr in v.GetCustomAttributes<FsmPatcherAttribute>())
                {
                    new FsmWatcher(CreateFilter(attr), d);
                }
            }
            var l = Languages;
            if (l != null)
            {
                Assembly ass = GetType().Assembly;
                foreach (var v in l)
                {
                    try
                    {
                        using (Stream stream = ass.GetManifestResourceStream(v.Item2))
                        {
                            I18n.AddLanguage(v.Item1, stream, false);
                        }
                    }
                    catch (Exception e)
                    {
                        LogError(e);
                    }
                }
                I18n.UseGameLanguage(DefaultLanguageCode);
            }
        }
        private readonly Lazy<I18n> _i18n = new Lazy<I18n>();
        protected virtual (Language.LanguageCode, string)[] Languages => null;
        protected virtual Language.LanguageCode DefaultLanguageCode => Language.LanguageCode.EN;
        public virtual string GetViewName() => GetName();
        public virtual bool FullScreen => false;

        public virtual void OnDebugDraw()
        {
            GUILayout.Label("Empty DebugView");
        }
    }
}
