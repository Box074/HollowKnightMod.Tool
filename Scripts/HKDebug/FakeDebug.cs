

namespace HKDebug;
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
        if (colors.TryGetValue(color, out var v))
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
    class DebugLineRenderer : MonoBehaviour
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

    public static void Init()
    {
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
    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0, bool depthTest = true, bool keepFixed = false)
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
        }
        else if (!line.renderer.gameObject.activeSelf)
        {
            line.renderer.gameObject.SetActive(true);
        }
    }
}

