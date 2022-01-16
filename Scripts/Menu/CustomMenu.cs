
namespace HKTool.Menu;

public abstract class CustomMenu
{
    public MenuScreen returnScreen { get; private set; }
    public string title { get; private set; }
    public MenuScreen menuScreen { get; private set; }
    public readonly MenuButton backButton;
    protected ContentArea content { get; private set; }
    public CustomMenu(MenuScreen returnScreen, string title)
    {
        var builder = MenuUtils.CreateMenuBuilderWithBackButton(title, returnScreen, out backButton);
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
            }, new RelLength(105), layout, (c1) =>
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
    protected void Back()
    {
        UIManager.instance.GoToDynamicMenu(returnScreen);
    }
    protected void Back(object _) => Back();
    protected abstract void Build(ContentArea contentArea);
}
