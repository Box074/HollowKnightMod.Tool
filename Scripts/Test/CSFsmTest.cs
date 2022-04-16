
namespace HKTool.Test;

class CSFsmTest : CSFsm<CSFsmTest>
{
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
        HKToolMod.logger.Log("Hello,World! This is TEST0(F)");
        yield return "TEST";
    }
    [FsmState]
    private IEnumerator Test1()
    {
        DefineEvent("FINISHED", nameof(Test0));
        yield return StartActionContent;

        HKToolMod.logger.Log("Hello,World! This is TEST1");
    }
}
