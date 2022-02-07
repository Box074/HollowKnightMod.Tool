
namespace HKTool.Menu;

public abstract class CustomMenu
{
    internal readonly static List<CustomMenu> menus = new();
    static CustomMenu()
    {
        On.UIManager.ShowMenu += (orig, self, menu) =>
        {
            try
            {
                foreach (var v in menus)
                {
                    if (v == null) continue;
                    if (v.autoRefresh)
                    {
                        try
                        {
                            v.Refresh();
                        }
                        catch (Exception e2)
                        {
                            Modding.Logger.LogError(e2);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Modding.Logger.LogError(e);
            }
            return orig(self, menu);
        };
    }
    public MenuScreen returnScreen { get; private set; }
    public string title { get; private set; }
    public MenuScreen menuScreen { get; private set; }
    protected ContentArea content { get; private set; }
    public virtual Font titleFont => MenuResources.TrajanBold;
    public virtual Font backButtonFont => MenuResources.TrajanBold;
    public virtual bool HasBackButton => true;
    [Obsolete]
    public virtual int itemCount { get; } = 1;
    public MenuButton backButton => _backButton;
    public bool autoRefresh { get; set; } = false;
    private MenuButton _backButton;
    protected readonly List<MenuSetting> needRefresh = new();
    private CustomMenu(string title)
    {
        menus.Add(this);
        var builder = MenuUtils.CreateMenuBuilder(title);
        var tf = titleFont ?? MenuResources.TrajanBold;
        var to = builder.MenuObject.transform.Find("Title");
        var tt = to.GetComponent<Text>();
        tt.font = tf;
        menuScreen = builder.Screen;
        if (HasBackButton)
        {
            builder.AddControls(new SingleContentLayout(new AnchoredPosition(new Vector2(0.5f, 0.5f),
           new Vector2(0.5f, 0.5f), new Vector2(0f, -64f))), (c) =>
           {
               c.AddMenuButton("BackButton", new()
               {
                   Label = Language.Language.Get("NAV_BACK", "MainMenu"),
                   CancelAction = Back,
                   SubmitAction = Back,
                   Proceed = true,
                   Style = MenuButtonStyle.VanillaStyle
               }, out _backButton);
               var bf = backButtonFont ?? MenuResources.TrajanBold;
               var l = _backButton.flashEffect.transform.parent;
               var lt = l.GetComponent<Text>();
               lt.font = bf;
           });
        }
        builder.AddContent(default(NullContentLayout), (c) =>
        {
            RegularGridLayout layout = RegularGridLayout.CreateVerticalLayout(105f, default(Vector2));
            c.AddScrollPaneContent(new ScrollbarConfig()
            {
                CancelAction = (_) => UIManager.instance.GoToDynamicMenu(returnScreen),
                Navigation = new Navigation()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = backButton,
                    selectOnDown = backButton
                },
                Position = new AnchoredPosition
                {
                    ChildAnchor = new Vector2(0f, 1f),
                    ParentAnchor = new Vector2(1f, 1f),
                    Offset = new Vector2(-310f, 0f)
                }
            }, new RelLength(105), layout, (c1) =>
                {
                    try
                    {
                        content = c1;
                        Build(c1);
                        var scrollPaneRt = content.ContentObject.GetComponent<RectTransform>();
                        RectTransformData.FromSizeAndPos(
                            new RelVector2(new RelLength(0f, 1f),
                                new RelLength((layout.Index == 0 ? 1 : layout.Index) * 105)),
                            new AnchoredPosition(new Vector2(0.5f, 1f),
                            new Vector2(0.5f, 1f), default(Vector2))).Apply(scrollPaneRt);
                    }
                    finally
                    {
                        content = null;
                    }
                });
        });
        builder.Build();
    }
    protected CustomMenu(MenuScreen returnScreen, string title) : this(title)
    {
        this.returnScreen = returnScreen;
    }
    protected MenuButton AddButton(string label, string desc, Action onSubmit, Font font = null)
    {
        content.AddMenuButton(label, new MenuButtonConfig()
        {
            SubmitAction = (_) => onSubmit?.Invoke(),
            CancelAction = Back,
            Label = label,
            Description = string.IsNullOrEmpty(desc) ? null : new DescriptionInfo()
            {
                Text = desc
            },
            Style = MenuButtonStyle.VanillaStyle
        }, out var menuButton);
        if (font != null)
        {
            var l = menuButton.flashEffect.transform.parent;
            var lt = l.GetComponent<Text>();
            lt.font = font;
            var d = menuButton.descriptionText;
            if(d != null)
            {
                var dt = d.GetComponent<Text>();
                dt.font = font;
            }
        }
        return menuButton;
    }
    public void Refresh()
    {
        foreach (var v in needRefresh) v.RefreshValueFromGameSettings();
    }
    protected void AddRefreshableComponent(MenuSetting menuSetting)
    {
        needRefresh.Add(menuSetting);
    }
    protected MenuOptionHorizontal AddOption(string label, string desc, string[] values,
        Action<MenuSetting, int> onChange, Func<MenuSetting, int> onRefresh, Font font = default)
    {
        content.AddHorizontalOption(label, new HorizontalOptionConfig()
        {
            CancelAction = Back,
            Label = label,
            Description = string.IsNullOrEmpty(desc) ? null : new DescriptionInfo()
            {
                Text = desc
            },
            Options = values,
            ApplySetting = (self, id) => onChange(self, id),
            RefreshSetting = (self, _) => self.optionList.SetOptionTo(onRefresh(self)),
            Style = HorizontalOptionStyle.VanillaStyle
        }, out var option);
        if (font != null)
        {
            option.optionText.font = font;
            var l = option.optionText.transform.parent.Find("Label");
            var lt = l.GetComponent<Text>();
            lt.font = font;

            var d = option.descriptionText;
            if (d != null)
            {
                var dt = d.GetComponent<Text>();
                dt.font = font;
            }
        }
        AddRefreshableComponent(option.menuSetting);
        return option;
    }
    protected void AddBoolOption(string label, string desc, Action<bool> onChange, Func<bool> onRefresh, Font font = default)
    {
        AddOption(label, desc, new string[] { "HKTool.Menu.Bool.False".Get(), "HKTool.Menu.Bool.True".Get() },
            (id) => { onChange(id == 1); }, () => onRefresh() ? 1 : 0, font);
    }
    protected MenuOptionHorizontal AddOption(string label, string desc, string[] values,
        Action<int> onChange, Func<int> onRefresh, Font font = default) => AddOption(label, desc, values, (_, id) => onChange(id),
            (_) => onRefresh(), font);
    protected MenuOptionHorizontal AddOption(string label, string desc,
        Action<int> onChange, Func<int> onRefresh, Font font = null, params string[] values) =>
            AddOption(label, desc, values, onChange, onRefresh, font);
    protected MenuOptionHorizontal AddOption(string label, string desc,
        Action<MenuSetting, int> onChange, Func<MenuSetting, int> onRefresh,
        Font font = null, params string[] values) => AddOption(label, desc, values, onChange, onRefresh, font);
    protected virtual void Back() => GoToMenu(returnScreen);
    protected void GoToMenu(MenuScreen menu)
    {
        if (menu == null) return;
        UIManager.instance.UIGoToDynamicMenu(menu);
    }
    protected void Back(object _) => Back();
    protected abstract void Build(ContentArea contentArea);
    public static implicit operator MenuScreen(CustomMenu customMenu) => customMenu.menuScreen;
}
