
namespace HKTool.Audio;

[Flags]
public enum MusicMask
{
    None = 0,
    MainOnly = 1 | IsVaildValue,
	MainAlt = 1 << 1,
	ActionOnly = 1 << 2 | IsVaildValue,
	SubOnly = 1 << 3 | IsVaildValue,
	TensionOnly = 1 << 4 | IsVaildValue,
	Extra = 1 << 5,
    Higher = 1 << 6,
    IsVaildValue = 1 << 7,

    All = MainOnly | MainAlt | ActionOnly | SubOnly | TensionOnly | Extra,
    NonExtra = MainOnly | MainAlt | ActionOnly | SubOnly | TensionOnly,
    NonExtraOrMainAlt = MainOnly | ActionOnly | SubOnly | TensionOnly,
    NoneExtraOrMainAltOrTension = MainOnly | ActionOnly | SubOnly,

    MainAndAction = MainOnly | ActionOnly,
    MainAndSub = MainOnly | SubOnly,
    ActionAndSub = ActionOnly | SubOnly,
    ActionNormal = MainOnly | SubOnly | ActionOnly,
    ActionHigher = ActionNormal | Higher,

    MainAltAndSub = SubOnly | MainAlt
}
