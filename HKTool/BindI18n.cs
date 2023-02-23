
namespace HKTool;

public interface IBindI18n
{
    void BindI18n(I18n i18n);
}
public class BindI18n : IBindI18n
{
    protected List<I18n> bindI18n = new();
    void IBindI18n.BindI18n(HKTool.I18n i18n)
    {
        if(!bindI18n.Contains(i18n)) bindI18n.Add(i18n);
    }
}
