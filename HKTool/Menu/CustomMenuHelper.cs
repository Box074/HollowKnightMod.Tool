
namespace HKTool.Menu;


public static class CutomMenuHelper
{
    public static Text? GetLabelText(this MenuButton button)
    {
        var l = button.flashEffect.transform.parent;
        return l?.GetComponent<Text>();
    }
    public static Text? GetLabelText(this MenuOptionHorizontal option)
    {
        var l = option.optionText.transform.parent.Find("Label");
        return l?.GetComponent<Text>();
    }
    public static Text? GetDescriptionText(this MenuSelectable button)
    {
        return button.descriptionText?.GetComponent<Text>();
    }
    public static void SetInteractable(this MenuButton element, bool interactable, string? desc = "")
    {
        var last = element.interactable;
        element.interactable = interactable;
        //if(last == interactable) return;
        var descText = element.gameObject.FindChild("Disable Desc Text");
        if (descText == null && !string.IsNullOrEmpty(desc))
        {
            descText = new GameObject("Disable Desc Text");
            descText.transform.parent = element.transform;
            descText.transform.localScale = new Vector3(1, 1, 1);
            var rt = descText.AddComponent<RectTransform>();
            var style = DescriptionStyle.MenuButtonSingleLineVanillaStyle;
            RectTransformData.FromSizeAndPos(style.Size,
                new AnchoredPosition(new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), default(Vector2)))
                .Apply(rt);
            Text descTextComponent = descText.AddComponent<Text>();
            descTextComponent.font = MenuResources.Perpetua;
            descTextComponent.fontSize = style.TextSize;
            descTextComponent.resizeTextMaxSize = style.TextSize;
            descTextComponent.alignment = style.TextAnchor;
            descTextComponent.text = desc;
            descTextComponent.supportRichText = true;
            descTextComponent.verticalOverflow = VerticalWrapMode.Overflow;
            descTextComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
        }
        if (descText is not null)
        {
            var text = descText.GetComponent<Text>();
            text.text = desc;
            descText.SetActive(!interactable);
            
        }
        element.GetDescriptionText()?.gameObject?.SetActive(interactable);
        foreach (var v in element.gameObject.GetComponentsInChildren<CanvasRenderer>())
        {
            if (interactable)
            {
                v.SetColor(Color.white);
            }
            else
            {
                v.SetColor(new Color(0.5f, 0.5f, 0.5f));
            }
        }
    }
}
