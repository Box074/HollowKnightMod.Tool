
namespace HKTool.ModMenu;

class TestMenu : CustomMenu 
{
    public static TestMenu? instance;
    public TestMenu(MenuScreen rs) : base(rs, "Test")
    {

    }
    public override bool DelayBuild => true;
    public override bool RebuildOnSwitchLanguage => false;
    public override Font titleFont => FontPerpetua;
    protected override void Build(ContentArea contentArea)
    {
        
    }
}
