using System;
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

namespace HKDebug
{
    public static class UnityExplorer
    {
        public static dynamic Instance { get; private set; } = null;
        public static Assembly ass = null;
        public static void Init()
        {

            try
            {
                ass = Assembly.Load("UnityExplorer.STANDALONE.Mono, Version=4.1.11.0, Culture=neutral, PublicKeyToken=null");
                if (ass == null)
                {
                    Modding.Logger.LogWarn("Not find UnityExplorer");
                    return;
                }
                TInspectUnderMouse = ass.GetType("UnityExplorer.Inspectors.InspectUnderMouse");
                /*Instance = ass.GetType("UnityExplorer.ExplorerStandalone").InvokeMember("CreateInstance",
                    BindingFlags.Public | BindingFlags.Static, null, null, Array.Empty<object>());*/
                FixColliderTest();
            }
            catch (Exception e)
            {
                Modding.Logger.Log(e);
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
                    MOnHitGameObject.Invoke(self, new object[] { col.gameObject });
                    return;
                }
            }
            if (lastGO != null)
            {
                lastGO = null;
                MClearHitData.Invoke(null, null);
            }

        }
        private static void FixColliderTest()
        {
            MOnHitGameObject = TInspectUnderMouse.GetMethod("OnHitGameObject", BindingFlags.NonPublic | BindingFlags.Instance);
            MClearHitData = TInspectUnderMouse.GetMethod("ClearHitData", BindingFlags.NonPublic | BindingFlags.Instance);
            HookEndpointManager.Add(TInspectUnderMouse.GetMethod("RaycastWorld", BindingFlags.NonPublic | BindingFlags.Instance),
               new Action<Action<object, Vector2>, object, Vector2>(HookRaycastWorld)
                );
        }
    }
}
