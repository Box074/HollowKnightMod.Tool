using System.Threading;

namespace HKTool.Debugger.Modules;

class LongFrameTracking : DebugModule
{
    private bool isEnabled = false;
    public override void OnEnable() 
    {
        _ = FrameWatch.Instance;
        lastFrameTime = DateTime.UtcNow;
        new Thread(FrameCheck).Start(Thread.CurrentThread);
    }
    public override void OnDisable()
    {
        isEnabled = false;
    }
    private void FrameCheck(object mt)
    {
        var mainThread = (Thread)mt;
        while(isEnabled)
        {
            Thread.Sleep(100);
            if((DateTime.UtcNow - lastFrameTime).TotalSeconds >= 1)
            {
                entryLongFrame = true;
                HKToolMod.logger.LogWarn("Detected frames that took too long");
#pragma warning disable CS0612
                HKToolMod.logger.LogWarn(new StackTrace(mainThread, true).ToString());
                while(entryLongFrame)
                {
                    Thread.Yield();
                }
            }
        }
    }
    public override bool CanRuntimeDisabled => true;
    public override bool CanRuntimeEnabled => true;
    private static DateTime lastFrameTime = DateTime.UtcNow;
    private static bool entryLongFrame = false;
    private class FrameWatch : SingleMonoBehaviour<FrameWatch>
    {
        private void Update()
        {
            lastFrameTime = DateTime.UtcNow;
            entryLongFrame = false;
        }
    }
}
