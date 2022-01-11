using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using MonoMod.RuntimeDetour.HookGen;
using HKTool;

namespace HKDebug
{
    public static class FakeDebug
    {
        public static bool enable = false;
        static List<LineData> lines = new List<LineData>();
        static LineData FindLine(Vector2 from, Vector2 to, Color color, float duartion) =>
            lines.Where(x => x.from == from)
                .Where(x => x.to == to)
                .Where(x => x.color == color)
                .FirstOrDefault(x => x.duration == duartion);
        static readonly Dictionary<Color, Material> colors = new Dictionary<Color, Material>();
        public static Material GetSharedColor(Color color)
        {
            if(colors.TryGetValue(color,out var v))
            {
                return v;
            }
            Material m = new Material(Shader.Find("Diffuse"));
            m.color = color;
            colors.Add(color, m);
            return m;
        }
        class LineData
        {
            public Vector2 from = Vector2.zero;
            public Vector2 to = Vector2.zero;
            public Color color = Color.white;
            public float duration = 0;
            public bool use = false;
            public bool keepFixed = false;
            public DebugLineRenderer renderer = null;
        }
        class DebugLineRenderer: MonoBehaviour
        {
            
            LineRenderer lr = null;
            LineData data = null;
            public void Init(LineData data)
            {
                this.data = data;
                data.use = true;
                data.renderer = this;

                lr = gameObject.AddComponent<LineRenderer>();
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;
                lr.sharedMaterial = GetSharedColor(data.color);
                lr.SetPositions(new Vector3[]{
                    data.from,
                    data.to
                    });
            }
            void FixedUpdate()
            {
                if (data != null)
                {
                    if (data.keepFixed)
                    {
                        if (!data.use)
                        {
                            data.keepFixed = false;
                            Destroy(gameObject);
                        }
                    }
                }
            }
            void LateUpdate()
            {
                if (!enable)
                {
                    if (data != null)
                    {
                        data.renderer = null;
                    }
                    Destroy(gameObject);
                }
                if (data != null)
                {

                    if (data.use)
                    {
                        data.use = false;
                    }
                    else
                    {
                        if (!data.keepFixed)
                        {
                            Destroy(gameObject);
                        }
                    }

                }
            }
        }
 /*     public static Menu.ButtonGroup group = new Menu.ButtonGroup();
        static bool chaseShow = false;
        static bool getposShow = false;
        static bool moveShow = false;*/
        public static void Init()
        {
 /*           Menu.MenuManager.AddButton(new Menu.ButtonInfo()
            {
                label = "Debug",
                submit = (_) => Menu.MenuManager.EnterGroup(group)
            });*/
            Menu.MenuManager.AddButton(new Menu.ButtonInfo()
            {
                label = "HKTool.Debug.EnableDebugDraw".Get(),
                submit = (but) =>
                {
                    enable = !enable;
                    but.label = !enable ? "HKTool.Debug.EnableDebugDraw".Get()
                    : "HKTool.Debug.DisableDebugDraw".Get();
                }
            });
#if DEBUG
            //group.AddButton(new Menu.ButtonInfo()
            //{
            //    label = "显示ChaseObjectV2",
            //    submit = (but) =>
            //    {
            //        chaseShow = !chaseShow;
            //        but.label = (!chaseShow ? "显示" : "隐藏") + "ChaseObjectV2";
            //    }
            //});
            //group.AddButton(new Menu.ButtonInfo()
            //{
            //    label = "显示GetPosition",
            //    submit = (but) =>
            //    {
            //        getposShow = !getposShow;
            //        but.label = (!getposShow ? "显示" : "隐藏") + "GetPosition";
            //    }
            //});
            //group.AddButton(new Menu.ButtonInfo()
            //{
            //    label = "显示Move",
            //    submit = (but) =>
            //    {
            //        moveShow = !moveShow;
            //        but.label = (!moveShow ? "显示" : "隐藏") + "Move";
            //    }
            //});
            //On.HutongGames.PlayMaker.Actions.SetPosition.DoSetPosition += SetPosition_DoSetPosition;
            //On.HutongGames.PlayMaker.Actions.iTweenMoveTo.DoiTween += ITweenMoveTo_DoiTween;
            //On.HutongGames.PlayMaker.Actions.ChaseObject.DoBuzz += ChaseObject_DoBuzz;
            //On.HutongGames.PlayMaker.Actions.ChaseObjectV2.DoChase += ChaseObjectV2_DoChase;
            //On.HutongGames.PlayMaker.Actions.GetPosition2D.DoGetPosition += GetPosition2D_DoGetPosition;
            //On.HutongGames.PlayMaker.Actions.GetPosition.DoGetPosition += GetPosition_DoGetPosition;
            //On.HutongGames.PlayMaker.Actions.GetAngleToTarget2D.DoGetAngle += GetAngleToTarget2D_DoGetAngle;
#endif
            HookEndpointManager.Add(typeof(Debug).GetMethod("DrawLine", new Type[]{
                typeof(Vector3),
                typeof(Vector3),
                typeof(Color),
                typeof(float),
                typeof(bool)
                }), new Action<Action<Vector3, Vector3, Color, float, bool>, Vector3, Vector3, Color, float, bool>(
                    (orig, s, e, c, d, dep) => DrawLine(s, e, c, d, dep)
                    ));
            HookEndpointManager.Add(typeof(Debug).GetMethod("DrawRay", new Type[]{
                typeof(Vector3),
                typeof(Vector3),
                typeof(Color),
                typeof(float),
                typeof(bool)
                }), new Action<Action<Vector3, Vector3, Color, float, bool>, Vector3, Vector3, Color, float, bool>(
                    (orig, s, dir, c, d, dep) =>
                    {
                        DrawLine(s, s + (dir * d), c, d, dep);
                    }
                    ));

        }
#if DEBUG
        //private static void ChaseObject_DoBuzz(On.HutongGames.PlayMaker.Actions.ChaseObject.orig_DoBuzz orig,
        //    HutongGames.PlayMaker.Actions.ChaseObject self)
        //{
        //    orig(self);
        //    if (chaseShow) DrawLine(self.gameObject.GetSafe(self).transform.position, self.target.Value.transform.position,
        //         Color.red, 0, true, true);
        //}

        //private static void SetPosition_DoSetPosition(On.HutongGames.PlayMaker.Actions.SetPosition.orig_DoSetPosition orig, 
        //    HutongGames.PlayMaker.Actions.SetPosition self)
        //{
        //    orig(self);
        //    if (moveShow) DrawLine(self.gameObject.GetSafe(self).transform.position,
        //        (self.space == Space.Self ? self.gameObject.GetSafe(self).transform.parent.position : Vector3.zero) +
        //         self.vector.Value + new Vector3(self.x.Value, self.y.Value, self.z.Value), Color.blue, 0, true, true);
        //}

        //private static void ITweenMoveTo_DoiTween(On.HutongGames.PlayMaker.Actions.iTweenMoveTo.orig_DoiTween orig, 
        //    HutongGames.PlayMaker.Actions.iTweenMoveTo self)
        //{
        //    orig(self);
        //    if (moveShow) DrawLine(self.gameObject.GetSafe(self).transform.position, self.vectorPosition.IsNone ?
        //         self.transformPosition.Value.transform.position : self.vectorPosition.Value, Color.blue, 0, true, true);
        //}

        //private static void GetAngleToTarget2D_DoGetAngle(On.HutongGames.PlayMaker.Actions.GetAngleToTarget2D.orig_DoGetAngle orig,
        //    HutongGames.PlayMaker.Actions.GetAngleToTarget2D self)
        //{
        //    orig(self);
        //    if (getposShow) DrawLine(self.Fsm.GameObject.transform.position, self.gameObject.GetSafe(self).transform.position,
        //         Color.red, 0, true, true);
        //}

        //private static void GetPosition2D_DoGetPosition(On.HutongGames.PlayMaker.Actions.GetPosition2D.orig_DoGetPosition orig, HutongGames.PlayMaker.Actions.GetPosition2D self)
        //{
        //    orig(self);
        //    if (getposShow) DrawLine(self.Fsm.GameObject.transform.position, self.gameObject.GetSafe(self).transform.position,
        //         Color.red, 0, true, true);
        //}

        //private static void GetPosition_DoGetPosition(On.HutongGames.PlayMaker.Actions.GetPosition.orig_DoGetPosition orig, 
        //    HutongGames.PlayMaker.Actions.GetPosition self)
        //{
        //    orig(self);
        //    if (getposShow) DrawLine(self.Fsm.GameObject.transform.position, self.gameObject.GetSafe(self).transform.position,
        //         Color.red, 0, true, true);
        //}

        //private static void ChaseObjectV2_DoChase(On.HutongGames.PlayMaker.Actions.ChaseObjectV2.orig_DoChase orig,
        //    HutongGames.PlayMaker.Actions.ChaseObjectV2 self)
        //{
        //    orig(self);
        //    if(chaseShow) DrawLine(self.gameObject.GetSafe(self).transform.position, self.target.Value.transform.position,
        //        Color.red, 0, true, true);
        //}
#endif

        public static void DrawLine(Vector3 start, Vector3 end,Color color, float duration = 0, bool depthTest = true, bool keepFixed = false)
        {
            if (!enable) return;
            if (lines.Count >= 100)
            {
                foreach (var v in lines.Where(x => x.renderer != null && !x.renderer.enabled))
                {
                    UnityEngine.Object.Destroy(v.renderer.gameObject);
                    v.renderer = null;
                }
                lines.RemoveAll(x => x.renderer == null);
            }
            LineData line = FindLine(start, end, color, duration);
            if (line == null)
            {
                line = new LineData()
                {
                    from = start,
                    to = end,
                    color = color,
                    duration = duration,
                    use = true
                };
                lines.Add(line);
            }
            line.keepFixed = keepFixed;
            line.use = true;
            if (line.renderer == null)
            {
                new GameObject("Debug DrawLine").AddComponent<DebugLineRenderer>().Init(line);
            }else if (!line.renderer.gameObject.activeSelf)
            {
                line.renderer.gameObject.SetActive(true);
            }
        }
    }
}
