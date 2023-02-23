
namespace HKTool.ModMenu;

class LogMenu : CustomMenu
{
    public override Font titleFont => MenuResources.Perpetua;
    public static HKToolSettings settings => HKToolMod.settings;
    #pragma warning disable CS8618
    public static LogMenu? instance;
    #pragma warning restore CS8618
    public override bool DelayBuild => true;
    public LogMenu(MenuScreen rs) : base(rs)
    {

    }
    public override string title => "HKTool.LogMenu.Title".Localize();
    protected override void Build(ContentArea contentArea)
    {
        AddOption("HKTool.LogMenu.MAPILogLevel".Localize(), "", Enum.GetNames(typeof(Modding.LogLevel)),
            (val) =>
            {
                ModHooks.GlobalSettings.LoggingLevel = (Modding.LogLevel)val;
                typeof(Modding.Logger).CreateReflectionObject().SetMemberData("_logLevel", (Modding.LogLevel)val);
            },
            () =>
            {
                return (int)ModHooks.GlobalSettings.LoggingLevel;
            }, MenuResources.Perpetua);
        for (int i = 0; i < 5; i++)
        {
            int id = i;
            AddOption(string.Format("HKTool.Menu.ULST".Localize(), Enum.GetName(typeof(LogType), (LogType)id)),
                "", Enum.GetNames(typeof(StackTraceLogType)),
            (int val) =>
            {
                Application.SetStackTraceLogType((LogType)id, (StackTraceLogType)val);
                if(HKToolMod.devSettings.UnityLogStackTraceType is not null)
                    HKToolMod.devSettings.UnityLogStackTraceType[id] = (StackTraceLogType)val;
            },
            () =>
            {
                return (int)Application.GetStackTraceLogType((LogType)id);
            }, MenuResources.Perpetua);
        }
        AddBoolOption("HKTool.LogMenu.RLogLabel".LocalizeFormat("Assert"), "",
                ref settings.DebugConfig.rUnityAssert, null, MenuResources.Perpetua);
        AddBoolOption("HKTool.LogMenu.RLogLabel".LocalizeFormat("Error"), "",
                ref settings.DebugConfig.rUnityError, null, MenuResources.Perpetua);
        AddBoolOption("HKTool.LogMenu.RLogLabel".LocalizeFormat("Exception"), "",
                ref settings.DebugConfig.rUnityException, null, MenuResources.Perpetua);
        AddBoolOption("HKTool.LogMenu.RLogLabel".LocalizeFormat("Log"), "",
                ref settings.DebugConfig.rUnityLog, null, MenuResources.Perpetua);
        AddBoolOption("HKTool.LogMenu.RLogLabel".LocalizeFormat("Warn"), "",
                ref settings.DebugConfig.rUnityWarn, null, MenuResources.Perpetua);
    }
}
