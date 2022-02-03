
namespace HKTool.SpecialBehaviour;

static class HookManager
{
    public static List<Type> attachHeroController = new();
    public static List<Type> attachHealthManager = new();
    static HookManager()
    {
        On.HeroController.Awake += (orig, self) =>
        {
            orig(self);
            foreach (var v in attachHeroController)
            {
                try
                {
                    self.gameObject.AddComponent(v);
                }
                catch (Exception e)
                {
                    Modding.Logger.LogError(e);
                }
            }
        };
        On.HealthManager.Awake += (orig, self) =>
        {
            orig(self);
            foreach (var v in attachHealthManager)
            {
                try
                {
                    self.gameObject.AddComponent(v);
                }
                catch (Exception e)
                {
                    Modding.Logger.LogError(e);
                }
            }
        };
    }
}
