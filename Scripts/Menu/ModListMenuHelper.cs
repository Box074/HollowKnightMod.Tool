
namespace HKTool.Menu;


public static class ModListMenuHelper
{
    public static ReflectionObject? modListMenu { get; private set; }
    private static Dictionary<string, MenuButton> modButtonList = new();
    public static MenuScreen modListMenuScreen => modListMenu?["screen"]?.As<MenuScreen>() ?? throw new NullReferenceException();
    private static bool modListBuildFinished = false;
    public static bool ModListMenuBuildComplete => modListBuildFinished;
    private static event Action<MenuScreen>? _onAfterBuildModListMenuComplete;
    public static event Action<MenuScreen> OnAfterBuildModListMenuComplete 
    {
        add
        {
            if(modListBuildFinished)
            {
                value(modListMenuScreen);
            }
            else
            {
                _onAfterBuildModListMenuComplete += value;
            }
        }
        remove
        {
            _onAfterBuildModListMenuComplete -= value;
        }
    }
    private static void AfterBuildModListMenuComplete()
    {
        modListBuildFinished = true;
        foreach(var v in modListMenuScreen.GetComponentsInChildren<MenuButton>())
        {
            if(v.name.EndsWith("_Settings"))
            {
                modButtonList.Add(v.name, v);
            }
        }
        if(_onAfterBuildModListMenuComplete is not null)
        {
            var screen = modListMenuScreen;
            foreach(var v in _onAfterBuildModListMenuComplete.GetInvocationList())
            {
                try
                {
                    v.DynamicInvoke(screen);
                }
                catch(Exception e)
                {
                    HKToolMod.logger.LogError(e);
                }
            }
        }
    }
    public static void HideModMenuButton(string name)
    {
        TestMenuLoad();
        FindButtonInMenuListMenu(name)?.gameObject.SetActive(false);
        RearrangeButtons();
    }
    public static void ShowModMenuButton(string name)
    {
        TestMenuLoad();
        FindButtonInMenuListMenu(name)?.gameObject.SetActive(true);
        RearrangeButtons();
    }
    private static void TestMenuLoad()
    {
        if(!modListBuildFinished) throw new InvalidOperationException("HKTool.Error.TryGetInstanceWithoutInit".GetFormat("ModListMenu"));
    }
    public static MenuButton? FindButtonInMenuListMenu(string modName)
    {
        if(modButtonList.TryGetValue(modName + "_Settings", out var v)) return v;
        return null;
    }
    public static void RearrangeButtons()
    {
        TestMenuLoad();
        var view = modButtonList.Values.FirstOrDefault()?.transform?.parent?.gameObject;
        if(view is null) return;
        var enableButton = new List<GameObject>();
        var disableButton = new List<GameObject>();
        for(int i = 0; i < view.transform.childCount ; i++)
        {
            var go = view.transform.GetChild(i).gameObject;
            if(!go.name.EndsWith("_Settings")) continue;
            if(go.activeSelf)
            {
                enableButton.Add(go);
            }
            else
            {
                disableButton.Add(go);
            }
        }
        var layout = RegularGridLayout.CreateVerticalLayout(105f, default(Vector2));
        foreach(var v in enableButton)
        {
            var rt = v.GetComponent<RectTransform>();
            if(rt is null) continue;
            layout.ModifyNext(rt);
        }
        foreach(var v in disableButton)
        {
            var rt = v.GetComponent<RectTransform>();
            if(rt is null) continue;
            layout.ModifyNext(rt);
        }
    }
    internal static void Init()
    {
        HookEndpointManager.Add(
            FindMethodBase("Modding.ModListMenu::.ctor"),
            //HReflectionHelper.FindType("Modding.ModListMenu")?.GetConstructor(Type.EmptyTypes)
            //?? throw new NullReferenceException(),
            (Action<object> orig, object self) =>
            {
                modListMenu = self.CreateReflectionObject();
                orig(self);
            }
        );
        HookEndpointManager.Add(
            //typeof(UIManager).GetMethod("add_EditMenus"),
            FindMethodBase("UIManager::add_EditMenus"),
            (Action<Action> orig, Action action) =>
            {
                orig(action);
                if (action == null) return;
                if (action.Method.DeclaringType.FullName != "Modding.ModListMenu") return;
                if (Modding.ReflectionHelper.GetField<UIManager, bool>(UIManager.instance,
                    "hasCalledEditMenus"))
                {
                    AfterBuildModListMenuComplete();
                }
                else
                {
                    HookEndpointManager.Add(
                        action.Method,
                        (Action<object> orig, object self) =>
                        {
                            orig(self);
                            AfterBuildModListMenuComplete();
                        }
                    );
                }
            }
        );
    }
}
