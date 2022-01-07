﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using UnityExplorer;
using HKTool.Reflection;

namespace HKTool.UnityExplorer
{
    public static class UnityExplorerLoader
    {
        public static ExplorerStandalone Instance { get; private set; } = null;
        public static Modding.SimpleLogger Logger { get; } = new Modding.SimpleLogger("UnityExplorer");
        public static Assembly ass = null;
        public static void Init()
        {

            try
            {
                Instance = ExplorerStandalone.CreateInstance(OnLog);
                ass = typeof(ExplorerStandalone).Assembly;
                TInspectUnderMouse = ass.GetType("UnityExplorer.Inspectors.MouseInspectors.WorldInspector");
                FixColliderTest();
            }
            catch (Exception e)
            {
                Modding.Logger.Log(e);
            }
        }
        private static void OnLog(string msg, LogType type)
        {
            switch (type)
            {
                case LogType.Assert:
                case LogType.Exception:
                case LogType.Error:
                    Logger.LogError(msg);
                    break;
                case LogType.Warning:
                    Logger.LogWarn(msg);
                    break;
                case LogType.Log:
                default:
                    Logger.Log(msg);
                    break;
            }
        }
        public static Type TInspectUnderMouse = null;
        private static Vector2 GetMousePos()
        {
            Vector3 v = Input.mousePosition;
            v.z = Camera.main.WorldToScreenPoint(Vector3.zero).z;
            return Camera.main.ScreenToWorldPoint(v);
        }
        static MethodInfo MOnHitGameObject = null;
        static MethodInfo MClearHitData = null;
        static GameObject lastGO = null;
        private static void HookRaycastWorld(Action<object, Vector2> _, object self, Vector2 _1)
        {
            Vector2 pos = GetMousePos();
            Collider2D[] c = Physics2D.OverlapPointAll(pos, Physics2D.AllLayers);
            if (c != null)
            {
                Collider2D col = c.FirstOrDefault(x => x.transform.position.z > 0);
                if (col != null)
                {
                    //if (lastGO == col.gameObject) return;
                    lastGO = col.gameObject;
                    MOnHitGameObject.FastInvoke(self, col.gameObject);
                    return;
                }
            }
            if (lastGO != null)
            {
                lastGO = null;
                MClearHitData.FastInvoke(null, null);
            }

        }
        private static void FixColliderTest()
        {
            MOnHitGameObject = TInspectUnderMouse.GetMethod("OnHitGameObject", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            MClearHitData = TInspectUnderMouse.GetMethod("ClearHitData", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            HookEndpointManager.Add(TInspectUnderMouse.GetMethod("UpdateMouseInspect", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance),
               new Action<Action<object, Vector2>, object, Vector2>(HookRaycastWorld)
                );
        }
    }
}
