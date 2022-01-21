
namespace HKTool.Menu;

public abstract class CustomMenu
{
    public CustomMenu returnScreenC { get; private set; }
    public MenuScreen returnScreen { get; private set; }
    public string title { get; private set; }
    public MenuScreen menuScreen { get; private set; }
    protected ContentArea content { get; private set; }
    public virtual int itemCount { get; } = 1;
    public MenuButton backButton => _backButton;
    private MenuButton _backButton;
    protected readonly List<MenuSetting> needRefresh = new();
    private CustomMenu(string title)
    {
        var builder = MenuUtils.CreateMenuBuilder(title);
        builder.AddControls(new SingleContentLayout(new AnchoredPosition(new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -64f))), (c) =>
            {
                c.AddMenuButton("BackButton", new()
                {
                    Label = "Back",
                    CancelAction = Back,
                    SubmitAction = Back,
                    Proceed = true,
                    Style = MenuButtonStyle.VanillaStyle
                }, out _backButton);
            });
        builder.AddContent(default(NullContentLayout), (c) =>
        {
            IContentLayout layout = RegularGridLayout.CreateVerticalLayout(105f, default(Vector2));
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
            }, new RelLength(105 * itemCount), layout, (c1) =>
                {
                    try
                    {
                        content = c1;
                        Build(c1);
                    }
                    finally
                    {
                        content = null;
                    }
                });
        });
        menuScreen = builder.Build();
    }
    protected CustomMenu(MenuScreen returnScreen, string title) : this(title)
    {
        this.returnScreen = returnScreen;
    }
    protected CustomMenu(CustomMenu returnScreen,string title) : this(title)
    {
        returnScreenC = returnScreen;
    }
    protected void AddButton(string label, string desc, Action onSubmit)
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
        });
    }
    public void Refresh()
    {
        foreach (var v in needRefresh) v.RefreshValueFromGameSettings();
    }
    protected MenuOptionHorizontal AddOption(string label, string desc, string[] values,
        Action<MenuSetting, int> onChange, Func<MenuSetting, int> onRefresh)
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
        option.menuSetting.RefreshValueFromGameSettings();
        needRefresh.Add(option.menuSetting);
        return option;
    }
    protected MenuOptionHorizontal AddOption(string label, string desc, string[] values,
        Action<int> onChange, Func<int> onRefresh) => AddOption(label, desc, values, (_, id) => onChange(id),
            (_) => onRefresh());
    protected MenuOptionHorizontal AddOption(string label, string desc,
        Action<int> onChange, Func<int> onRefresh, params string[] values) => AddOption(label,desc,values,onChange, onRefresh);
    protected MenuOptionHorizontal AddOption(string label, string desc,
        Action<MenuSetting, int> onChange, Func<MenuSetting, int> onRefresh, params string[] values) => AddOption(label, desc, values, onChange, onRefresh);
    protected virtual void Back() => GoToMenu(returnScreen ?? returnScreenC);
    protected void GoToMenu(MenuScreen menu)
    {
        if(menu == null) return;
        UIManager.instance.UIGoToDynamicMenu(menu);
    }
    protected void Back(object _) => Back();
    protected abstract void Build(ContentArea contentArea);
    public static implicit operator MenuScreen(CustomMenu customMenu) => customMenu.menuScreen;
}
