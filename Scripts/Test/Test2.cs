
namespace HKTool.Test;

class Test2 {
    private static Test2 instance = new();
    private bool test = false;
    private void PrintTest() {
        HKToolMod.logger.LogError($"TTTTT: {test}");
    }
}
