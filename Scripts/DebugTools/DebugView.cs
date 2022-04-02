
namespace HKTool.DebugTools;
class DebugView : MonoBehaviour
{
    public static DebugView Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Camera.main.gameObject.AddComponent<DebugView>();
            }
            return _instance;
        }
    }
    public static Vector2 vpos = new Vector2(45, 72);
    public static Vector2 vsize = new Vector2(Screen.width * 0.3f, Screen.height - 90);
    public static Rect wrect = new Rect(vpos, vsize);
    private static DebugView? _instance;
    public static bool IsEnable { get; set; } = false;
    public static readonly List<IDebugViewBase> debugViews = new List<IDebugViewBase>();

    public static int select = 0;
    public Vector2 s0 = Vector2.zero;
    public static void Init()
    {
        TestInstance();
        Modding.ModHooks.HeroUpdateHook += TestInstance;
        Modding.ModHooks.CursorHook += ModHooks_CursorHook;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private static void ModHooks_CursorHook()
    {
        if (GameManager.instance.isPaused)
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = IsEnable;
        }
    }

    private static void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        TestInstance();
    }

    private static void TestInstance()
    {
        if (_instance == null)
        {
            _instance = Camera.main.gameObject.AddComponent<DebugView>();
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Home)) IsEnable = !IsEnable;
        if (IsEnable)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                s0 = Vector2.zero;
                select++;
            }
        }
    }
    private void WindowDraw(int id)
    {

        var d = debugViews[select];
        GUILayout.BeginVertical();

        if (GUILayout.Button(string.Format("HKTool.Debug.TopTitle".Get(), d.GetViewName(), select + 1, debugViews.Count)))
        {
            s0 = Vector2.zero;
            select++;
            if (select >= debugViews.Count) select = 0;

        }

        s0 = GUILayout.BeginScrollView(s0);

        try
        {
            d.OnDebugDraw();
        }
        catch (Exception e)
        {
            Modding.Logger.LogError(e);
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUI.DragWindow();
    }
    private void OnGUI()
    {
        if (!IsEnable || debugViews.Count == 0) return;
        if (select >= debugViews.Count) select = 0;
        var d = debugViews[select];
        GUI.color = Color.white;

        bool fullScreen = d.FullScreen;
        if (fullScreen)
        {
            d.OnDebugDraw();
        }
        else
        {
            wrect = GUILayout.Window(1, wrect, WindowDraw, "");
        }
    }
}

