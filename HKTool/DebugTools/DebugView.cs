using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HKTool.DebugTools
{
    class DebugView : MonoBehaviour
    {
        public static DebugView Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Camera.main.gameObject.AddComponent<DebugView>();
                }
                return _instance;
            }
        }
        private static DebugView _instance;
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
        private void Awake()
        {
            background = new Texture2D(1, 1);
            background.SetPixel(0, 0, new Color(1, 1, 1, 0.5f));
        }
        private Texture2D background = null;
        private void OnGUI()
        {
            if (!IsEnable || debugViews.Count == 0) return;
            if (select >= debugViews.Count) select = 0;
            var d = debugViews[select];
            var size = new Rect(45, 45, Screen.width * 0.3f, Screen.height - 90);
            
            GUI.DrawTexture(size, background);
            GUI.color = Color.black;
            GUILayout.BeginArea(size);
            GUILayout.BeginVertical();

            if (GUILayout.Button(string.Format("HKTool.Debug.TopTitle".Get(), d.GetModName(), select + 1, debugViews.Count)))
            {
                s0 = Vector2.zero;
                select++;
                if (select >= debugViews.Count) select = 0;

            }
            
            s0 = GUILayout.BeginScrollView(s0);

            try
            {
                d.OnDebugDraw();
            }catch(Exception e)
            {
                Modding.Logger.LogError(e);
            }
 
            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            GUILayout.EndArea();
        }
    }
}
