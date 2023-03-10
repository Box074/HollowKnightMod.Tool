
namespace HKTool;
public abstract class ModBase : Mod, IHKToolMod
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    internal protected class PreloadSharedAssetsAttribute : Attribute
    {
        public PreloadSharedAssetsAttribute(string name, Type? type = null)
        {
            inResources = true;
            this.name = name;
            this.targetType = type;
        }
        public PreloadSharedAssetsAttribute(int id, string name, Type? type = null) : this(name, type)
        {
            inResources = id == 0;
            this.id = id;
        }
        public PreloadSharedAssetsAttribute(int id, string name, bool cloneOne, Type? type = null) : this(id, name, type)
        {
            this.cloneOne = cloneOne;
        }
        [Obsolete]
        public PreloadSharedAssetsAttribute(string scene, string name, Type? type = null) : this(Array.IndexOf(sceneNames, scene), name, type)
        {

        }
        public Type? targetType;
        public string name;
        public bool inResources;
        public int id;
        public bool cloneOne;
    }
    public const string compileVersion = "2.1.0";
    private static int _currentmapiver = (int)(HReflectionHelper.FindType("Modding.ModHooks")!
        .GetField("_modVersion", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
    public static int CurrentMAPIVersion => _currentmapiver;
    public static readonly string[] sceneNames;
    static ModBase()
    {
        InitManager.CheckInit();

        var sceneNames = new List<string>();
        var sceneCount = USceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            sceneNames.Add(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
        }
        ModBase.sceneNames = sceneNames.ToArray();
    }
    private static FsmFilter CreateFilter(FsmPatcherAttribute attr)
    {
        if (attr.useRegex)
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
    public virtual string MenuButtonName => GetName();
    public virtual string DisplayName => GetName();
    public virtual Font MenuButtonLabelFont => MenuResources.TrajanBold;
    public virtual Version? HKToolMinVersion
    {
        get
        {
            var ver = GetType().Assembly.GetCustomAttribute<NeedHKToolVersionAttribute>()?.version;
            if (ver == null) return null;
            return Version.Parse(ver);
        }
    }
    public virtual List<(string, string)>? needModules => null;
    public MenuButton? ModListMenuButton { get; private set; }
    protected virtual bool ShowDebugView => true;
    private void CheckHKToolVersion(string? name = null)
    {
        if (HKToolMinVersion is null) return;
        var hkv = typeof(HKToolMod2).Assembly.GetName().Version;
        if (hkv < HKToolMinVersion)
        {
            TooOldDependency("HKTool", HKToolMinVersion);
        }
    }
    public override string GetVersion()
    {
        return GetType().Assembly.GetName().Version.ToString() + "-" + sha1 +
            (DebugManager.IsDebug(this) ? "-Dev" : "");
    }
    public void HideButtonInModListMenu()
    {
        if (ModListMenuButton is null) throw new InvalidOperationException();
        ModListMenuButton.gameObject.SetActive(false);
        ModListMenuHelper.RearrangeButtons();
    }
    public void ShowButtonInModListMenu()
    {
        if (ModListMenuButton is null) throw new InvalidOperationException();
        ModListMenuButton.gameObject.SetActive(true);
        ModListMenuHelper.RearrangeButtons();
    }
    protected void MissingDependency(string name)
    {
        var err = "HKTool.Error.NeedLibrary"
                .LocalizeFormat(name);
        ModManager.modErrors.Add((GetName(), err));
        throw new NotSupportedException(err);
    }
    protected void TooOldDependency(string name, Version needVersion)
    {
        var err = "HKTool.Error.NeedLibraryVersion"
                .LocalizeFormat(name, needVersion.ToString());
        ModManager.modErrors.Add((GetName(), err));
        throw new NotSupportedException(err);
    }
    protected void TooOldDependency(string name, string needVersion)
    {
        var err = "HKTool.Error.NeedLibraryVersion"
                .LocalizeFormat(name, needVersion.ToString());
        ModManager.modErrors.Add((GetName(), err));
        throw new NotSupportedException(err);
    }
    public virtual void OnCheckDependencies()
    {

    }
    protected virtual void AfterCreateModListButton(MenuButton button)
    {
        var labelT = button.GetLabelText();
        if (labelT is null) return;
        labelT.text = MenuButtonName;
        labelT.font = MenuButtonLabelFont;
    }
    public virtual I18n I18n => _i18n.Value;
    public byte[]? GetEmbeddedResource(string name) => GetType().Assembly.GetManifestResourceBytes(name);
    public Texture2D LoadTexture2D(string name)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(GetEmbeddedResource(name));
        return tex;
    }
    public AssetBundle LoadAssetBundle(string name)
    {
        return AssetBundle.LoadFromMemory(GetEmbeddedResource(name));
    }

    public static ModBase? FindMod(Type type)
    {
        if (ModManager.instanceMap.TryGetValue(type, out var v)) return v;
        return null;
    }
    public readonly string sha1;
    protected static bool HaveAssembly(string name)
    {
        foreach (var v in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (v.GetName().Name == name) return true;
        }
        return false;
    }
    protected void CheckAssembly(string name, Version minVer)
    {
        foreach (var v in AppDomain.CurrentDomain.GetAssemblies())
        {
            var n = v.GetName();
            if (n.Name == name)
            {
                if (n.Version < minVer) TooOldDependency(name, minVer);
                return;
            }
        }
        MissingDependency(name);
    }
    private byte[] GetAssemblyBytes()
    {
        return File.ReadAllBytes(GetType().Assembly.Location);
    }
    private void LoadPreloadResource(List<PreloadAssetInfo> table)
    {
        var batch = new Dictionary<Type, List<PreloadAssetInfo>>();
        foreach (var v2 in table)
        {
            if (!batch.TryGetValue(v2.targetType, out var v3))
            {
                v3 = new();
                batch.Add(v2.targetType, v3);
            }
            v3.Add(v2);
            LogFine($"Request {v2.name} ({v2.targetType.Name})");
        }
        foreach (var g in batch)
        {
            var type = g.Key;
            var list = g.Value;

            HashSet<string> matchNames = new();
            foreach (var v in list)
            {
                matchNames.Add(v.name);
            }
            var matchobjects = ResourcesUtils.FindAssets(matchNames, type);
            foreach (var v in list)
            {
                try
                {
                    if (!matchobjects.TryGetValue(v.name, out var obj) || obj == null)
                    {

                        LogError($"{v.name}({type.Name}) not found");
                        v.cb.Invoke(null);
                        continue;
                    }
                    var oin = obj;
                    if (v.cloneOne)
                    {
                        if (v.targetType == typeof(GameObject))
                        {
                            oin = ((GameObject)oin).CloneAsPrefab();
                        }
                        else if (v.targetType.IsSubclassOf(typeof(Component)))
                        {
                            oin = ((Component)oin).CloneAsPrefab();
                        }
                        else
                        {
                            oin = UObject.Instantiate(oin);
                            oin.name = obj.name;
                        }
                    }
                    LogFine($"Found {v.name} ({type.Name})");
                    v.cb.Invoke(oin);
                }
                catch (Exception e)
                {
                    LogError(e);
                }
            }
        }

    }
    void IHKToolMod.HookInit(PreloadObject go)
    {
        if (assetpreloads.TryGetValue(0, out var inresources))
        {
            LoadPreloadResource(inresources);
        }
        foreach (var v in preloads)
        {
            if (go is null)
            {
                if (v.Value.throwExceptionOnMissing) throw new MissingPreloadObjectException(v.Value.scene, v.Value.name);
                continue;
            }
            if (!go.TryGetValue(v.Value.scene, out var scene))
            {
                if (v.Value.throwExceptionOnMissing) throw new MissingPreloadObjectException(v.Value.scene, v.Value.name);
                LogWarn("Missing Scene: " + v.Value.scene);
                continue;
            }
            if (!scene.TryGetValue(v.Value.name, out var obj))
            {
                if (v.Value.throwExceptionOnMissing) throw new MissingPreloadObjectException(v.Value.scene, v.Value.name);
                LogWarn("Missing Object: " + v.Value.name);
                continue;
            }
            try
            {
                if (v.Value.cloneOne)
                {
                    obj = obj.CloneAsPrefab();
                    UObject.Destroy(obj);
                }
                v.Key.Invoke(obj);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }
    }
    private List<(string, string)> HookGetPreloads(List<(string, string)> preloads)
    {
        preloads = preloads ?? new();
        foreach (var v in this.preloads) preloads.Add((v.Value.scene, v.Value.name));
        return preloads;
    }
    internal record PreloadAssetInfo(string name, Type targetType, bool cloneOne, Action<UObject?> cb);
    public override (string, Func<IEnumerator>)[] PreloadSceneHooks()
    {
        IEnumerator PreloadAssets(List<PreloadAssetInfo> assets)
        {
            LoadPreloadResource(assets);
            yield break;
        }
        return assetpreloads.Select(x => (sceneNames[x.Key], (Func<IEnumerator>)PreloadAssets(x.Value).AsEnumerable().GetEnumerator)).ToArray();
    }
    private void CheckHookGetPreloads()
    {
        if (needHookGetPreloads)
        {
            var gpM = GetType().GetTypeInfo().DeclaredMethods.FirstOrDefault(x => x.Name == "GetPreloadNames"
                && x.GetParameters().Length == 0 &&
                x.ReturnType == typeof(List<(string, string)>));
            if (gpM is not null)
            {
                HookEndpointManager.Add(
                    gpM,
                    (Func<ModBase, List<(string, string)>> orig, ModBase self) =>
                    {
                        return HookGetPreloads(orig(self));
                    }
                );
            }
            else
            {
                ModManager.hookGetPreloads[this] = HookGetPreloads;
            }
        }
    }
    private bool needHookGetPreloads = false;
    internal record PreloadGameObjectInfo(string scene, string name, bool throwExceptionOnMissing, bool cloneOne);
    internal Dictionary<Action<GameObject?>, PreloadGameObjectInfo> preloads = new();
    internal Dictionary<int, List<PreloadAssetInfo>> assetpreloads = new();
    protected void AddPreloadSharedAsset(int? id, string name, Type type, bool cloneOne, Action<UObject?> callback)
    {
        if (ModLoaderR.LoadState.HasFlag(ModLoadStateR.Preloaded)) throw new InvalidOperationException();
        if (!typeof(UObject).IsAssignableFrom(type)) return;

        var sceneId = id ?? 0;

        var list = assetpreloads.TryGetOrAddValue(sceneId, () => new());
        needHookGetPreloads = true;
        list.Add(new(name, type, cloneOne, callback));
    }
    private void CheckPreloads()
    {
        Action<T> CreateSetter<T>(MemberInfo m)
        {
            if (m is FieldInfo f) return val => f.FastSet(this, val);
            if (m is PropertyInfo p) return val => p.FastSet(this, val);
            if (m is MethodInfo met) return val => met.FastInvoke(this, val);
            return _ => { };
        }
        var t = GetType();
        foreach (var v in t.GetMembers(HReflectionHelper.All))
        {
            if (v is not (FieldInfo or MethodInfo or PropertyInfo)) continue;
            if (v is MethodInfo m && m.GetParameters().Length != 1) continue;
            var p = v.GetCustomAttribute<PreloadAttribute>();
            if (p is not null)
            {
                preloads.Add(CreateSetter<GameObject?>(v), new(p.sceneName, p.objPath, p.throwExceptionOnMissing, p.setActive));
                needHookGetPreloads = true;
                continue;
            }
            var pa = v.GetCustomAttribute<PreloadSharedAssetsAttribute>();
            if (pa is not null)
            {
                var targetType = pa.targetType;
                if (targetType is null)
                {
                    if (v is FieldInfo f) targetType = f.FieldType;
                    else if (v is MethodInfo method) targetType = method.GetParameters()[0].ParameterType;
                    else if (v is PropertyInfo prop) targetType = prop.PropertyType;
                    else continue;
                }
                AddPreloadSharedAsset(pa.inResources ? null : pa.id, pa.name, targetType, pa.cloneOne, CreateSetter<UObject?>(v));
            }
        }
    }
    protected ModBase(string? name = null) : this(name!, false)
    {

    }
    private ModBase(string name, bool _) : base(name)
    {
        CheckHKToolVersion(name);
        OnCheckDependencies();

        ModManager.NewMod(this);

        sha1 = BitConverter.ToString(
            System.Security.Cryptography.SHA1
                .Create().ComputeHash(GetAssemblyBytes()))
                .Replace("-", "").ToLowerInvariant().Substring(0, 6);

        if (this is ICustomMenuMod || this is IMenuMod)
        {
            ModListMenuHelper.OnAfterBuildModListMenuComplete += (_) =>
            {
                ModListMenuButton = ModListMenuHelper.FindButtonInMenuListMenu(GetName());
                if (ModListMenuButton is null) return;
                var buttonText = ModListMenuButton.GetLabelText();
                if (buttonText is not null)
                {
                    buttonText.text = MenuButtonName;
                    buttonText.font = MenuButtonLabelFont;
                }
                AfterCreateModListButton(ModListMenuButton);
            };
        }

        if (this is IDebugViewBase @base && ShowDebugView)
        {
            DebugView.debugViews.Add(@base);
        }
        foreach (var v in GetType().GetMethods(HReflectionHelper.All))
        {
            if (v.ReturnType != typeof(void) || v.GetParameters().Length != 1) continue;
            var fp = v.GetParameters()[0].ParameterType;
            foreach (var attr in v.GetCustomAttributes<FsmPatcherAttribute>())
            {
                if (fp == typeof(Fsm)) new FsmWatcher(CreateFilter(attr), fsm => v.FastInvoke(this, fsm!.Fsm));
                if (fp == typeof(PlayMakerFSM)) new FsmWatcher(CreateFilter(attr), fsm => v.FastInvoke(this, fsm));
                if (fp == typeof(FSMPatch)) new FsmWatcher(CreateFilter(attr),
                    fsm =>
                    {
                        using (var p = fsm!.Fsm.CreatePatch()) v.FastInvoke(this, p);
                    });
            }
        }
        _i18n = new Lazy<I18n>(
                    () =>
                        new(GetName(), Path.GetDirectoryName(GetType().Assembly.Location), (LanguageCode)DefaultLanguageCode)
                );
        InitI18n();
        CheckPreloads();
        CheckHookGetPreloads();
    }
    private void InitI18n()
    {

#pragma warning disable CS0618
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
        }
#pragma warning restore CS0618
        var lex = LanguagesEx;
        if (lex != null)
        {
            Assembly ass = GetType().Assembly;
            foreach (var v in lex)
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
        }
        if (l is not null || lex is not null)
        {
            I18n.TrySwitch();
            if (this is ICustomMenuMod || this is IMenuMod)
            {
                I18n.OnLanguageSwitch += () =>
                {
                    var buttonText = ModListMenuButton?.GetLabelText();
                    if (buttonText is not null) buttonText.text = MenuButtonName;
                };
            }
        }
    }
    private readonly Lazy<I18n> _i18n;
    [Obsolete("Please override LanguagesEx instead of Languages")]
    protected virtual (Language.LanguageCode, string)[]? Languages => null;
    protected virtual List<(SupportedLanguages, string)>? LanguagesEx => null;
    protected virtual SupportedLanguages DefaultLanguageCode => SupportedLanguages.EN;
    public virtual string GetViewName() => GetName();
    public virtual bool FullScreen => false;

    public virtual void OnDebugDraw()
    {
        GUILayout.Label("Empty DebugView");
    }
}
public abstract class ModBase<T> : ModBase where T : ModBase<T>
{
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindMod(typeof(T)) as T;
                if (_instance == null)
                {
                    throw new InvalidOperationException("HKTool.Error.GetModInstaceBeforeLoad".LocalizeFormat(typeof(T).Name));
                }
            }
            return _instance;
        }
    }
    private static T? _instance = null;
    public ModBase(string? name = null) : base(name)
    {
        _instance = (T)this;
    }
}
public abstract class ModBaseWithSettings<TGlobalSettings, TLocalSettings> : ModBase where TGlobalSettings : new()
        where TLocalSettings : new()
{
    public virtual TLocalSettings localSettings { get; protected set; } = new();
    public TLocalSettings OnSaveLocal() => localSettings;
    public void OnLoadLocal(TLocalSettings s) => localSettings = s;
    public virtual TGlobalSettings globalSettings { get; protected set; } = new();
    public TGlobalSettings OnSaveGlobal() => globalSettings;
    public void OnLoadGlobal(TGlobalSettings s) => globalSettings = s;
}
public abstract class ModBaseWithSettings<T, TGlobalSettings, TLocalSettings> : ModBase<T> where TGlobalSettings : new()
        where TLocalSettings : new() where T : ModBaseWithSettings<T, TGlobalSettings, TLocalSettings>
{
    public virtual TLocalSettings localSettings { get; protected set; } = new();
    public TLocalSettings OnSaveLocal() => localSettings;
    public void OnLoadLocal(TLocalSettings s) => localSettings = s;
    public virtual TGlobalSettings globalSettings { get; protected set; } = new();
    public TGlobalSettings OnSaveGlobal() => globalSettings;
    public void OnLoadGlobal(TGlobalSettings s) => globalSettings = s;
}

