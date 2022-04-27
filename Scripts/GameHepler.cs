
namespace HKTool;

public static class GameHelper
{
    public static bool TryGetHeroController(out HeroController heroController)
    {
        return (heroController = HeroController.SilentInstance) != null;
    }
    public static bool TryGetGameManager(out GameManager gameManager)
    {
        return (gameManager = GameManager.UnsafeInstance) != null;
    }
    public static bool TryGetTheClosestEnemy(Vector2 pos, float maxDistance, 
        bool ignoreDead,
        bool needCollider,
        out HealthManager healthManager)
    {
        IEnumerable<HealthManager> enemies = UObject.FindObjectsOfType<HealthManager>(true);
        if(ignoreDead)
        {
            enemies = enemies.Where(x => (!x.isDead) && (x.hp > 0));
        }
        if(needCollider)
        {
            enemies = enemies.Where(x => x.GetComponents<Collider2D>().Any(c => c.enabled));
        }
        healthManager = enemies.OrderBy(
            x => (((Vector2)x.gameObject.transform.position) - pos).magnitude
            ).FirstOrDefault();
        return healthManager != null;
    }
}
