
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
    public override string title => "HKTool.LogMenu.Title".Get();
    protected override void Build(ContentArea contentArea)
    {
        AddOption("HKTool.LogMenu.MAPILogLevel".Get(), "", Enum.GetNames(typeof(Modding.LogLevel)),
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
            AddOption(string.Format("HKTool.Menu.ULST".Get(), Enum.GetName(typeof(LogType), (LogType)id)),
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
        AddBoolOption("HKTool.LogMenu.RLogLabel".GetFormat("Assert"), "",
            (val) => 
            {
                settings.DebugConfig.rUnityAssert = val;
            },
            () =>
            {
                return settings.DebugConfig.rUnityAssert;
            }, MenuResources.Perpetua);
        AddBoolOption("HKTool.LogMenu.RLogLabel".GetFormat("Error"), "",
            (val) =>
            {
                settings.DebugConfig.rUnityError = val;
            },
            () =>
            {
                return settings.DebugConfig.rUnityError;
            }, MenuResources.Perpetua);
        AddBoolOption("HKTool.LogMenu.RLogLabel".GetFormat("Exception"), "",
            (val) =>
            {
                settings.DebugConfig.rUnityException = val;
            },
            () =>
            {
                return settings.DebugConfig.rUnityException;
            }, MenuResources.Perpetua);
        AddBoolOption("HKTool.LogMenu.RLogLabel".GetFormat("Log"), "",
            (val) =>
            {
                settings.DebugConfig.rUnityLog = val;
            },
            () =>
            {
                return settings.DebugConfig.rUnityLog;
            }, MenuResources.Perpetua);
        AddBoolOption("HKTool.LogMenu.RLogLabel".GetFormat("Warn"), "",
            (val) =>
            {
                settings.DebugConfig.rUnityWarn = val;
            },
            () =>
            {
                return settings.DebugConfig.rUnityWarn;
            }, MenuResources.Perpetua);
    }
}
