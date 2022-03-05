
namespace HKDebug.HitBox;
[Serializable]
public class HitBoxConfig
{
    public List<HitBoxColor> colors = new List<HitBoxColor>();
}

[Serializable]
public class HitBoxColor
{
    [JsonConverter(typeof(StringEnumConverter))]
    public GlobalEnums.PhysLayers layer = GlobalEnums.PhysLayers.DEFAULT;
    public List<string> needComponents = new List<string>();
    public List<string> needPlayMakerFSMs = new List<string>();
    public float r = 0;
    public float g = 0;
    public float b = 0;
    public float index = 0;
    //public bool includeDisable = false;
}

