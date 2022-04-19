
namespace HKTool.Test;

class CSFsmTest : CSFsm<CSFsmTest>
{
    [FsmVar]
    public FsmInt var0 = new();
    [FsmVar]
    public FsmInt var2 = new();
    [FsmVar]
    public FsmInt var3 = new();
    [FsmVar]
    public FsmInt var1 = new();
    [FsmState]
    private IEnumerator Test0()
    {
        DefineEvent("TEST", nameof(Test1));
        FsmStateAction testAction = FSMHelper.CreateMethodAction((_) =>
        {
            HKToolMod.logger.Log("Hello,World! This is Test Action");
        });
        FsmStateAction testAction2 = FSMHelper.CreateMethodAction((_) =>
        {
            HKToolMod.logger.Log("Hello,World! This is Test Action2");
        });
        HKToolMod.logger.Log("Hello,World! Test 0 Init");

        yield return StartActionContent;

        HKToolMod.logger.Log("Hello,World! This is TEST0");
        yield return new WaitForSeconds(0.5f);
        InvokeAction(testAction);
        InvokeAction(testAction2);
        HKToolMod.logger.Log("Hello,World! This is TEST0(F)" + var0.Value);
        var0.Value = var0.Value + 1;
        yield return "TEST2";
    }
    [FsmState]
    private IEnumerator Test1()
    {
        DefineEvent("FINISHED", nameof(Test0));
        yield return StartActionContent;

        HKToolMod.logger.Log("Hello,World! This is TEST1");
    }
    [FsmState]
    private IEnumerator Test2()
    {
        DefineGlobalEvent("TEST2");
        DefineEvent("TEST", "Test1");
        yield return StartActionContent;

        HKToolMod.logger.Log("Hello,World! This is TEST2");
        yield return "TEST";
    }
}
