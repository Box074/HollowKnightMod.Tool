
namespace HKTool.Menu;

public static class CutomMenuHelper
{
    public static Text GetLabelText(this MenuButton button)
    {
        var l = button.flashEffect.transform.parent;
        return l?.GetComponent<Text>();
    }
    public static Text GetLabelText(this MenuOptionHorizontal option)
    {
        var l = option.optionText.transform.parent.Find("Label");
        return l?.GetComponent<Text>();
    }
    public static Text GetDescriptionText(this MenuButton button)
    {
        return button.descriptionText?.GetComponent<Text>();
    }
    public static Text GetDescriptionText(this MenuOptionHorizontal option)
    {
        return option.descriptionText?.GetComponent<Text>();
    }
}
