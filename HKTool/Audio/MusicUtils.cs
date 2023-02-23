
namespace HKTool.Audio;

public static class MusicUtils
{
    public static MusicMask GetMask(params MusicChannels[] channels)
    {
        MusicMask result = MusicMask.None;
        foreach(var v in channels)
        {
            result |= v switch
            {
                MusicChannels.Main => MusicMask.MainOnly,
                MusicChannels.MainAlt => MusicMask.MainAlt,
                MusicChannels.Action => MusicMask.ActionOnly,
                MusicChannels.Sub => MusicMask.SubOnly,
                MusicChannels.Tension => MusicMask.TensionOnly,
                MusicChannels.Extra => MusicMask.Extra,
                _ => MusicMask.None
            };
        }
        return result;
    }
    private static Dictionary<MusicMask, string> audioMixerTable = new()
    {
        [MusicMask.All] = "HK Decline 6",
        [MusicMask.NonExtra] = "HK Decline 5",
        [MusicMask.NonExtraOrMainAlt] = "HK Decline 4",
        [MusicMask.ActionNormal] = "HK Decline 3",
        [MusicMask.ActionHigher] = "Action",
        [MusicMask.MainAndSub] = "Normal",
        [MusicMask.ActionAndSub] = "Action and Sub",
        [MusicMask.MainAndAction] = "HK Decline 2",

        [MusicMask.MainOnly] = "Main Only",
        [MusicMask.None] = "Silent",
        
        [MusicMask.ActionOnly] = "Action Only",
        [MusicMask.TensionOnly] = "Tension Only",
        [MusicMask.SubOnly] = "Sub Area",

        [MusicMask.MainAltAndSub] = "Normal Alt"
    };
    public static bool IsVaild(this MusicMask mask)
    {
        if(mask == MusicMask.None) return true;
        if(!mask.HasFlag(MusicMask.IsVaildValue)) return false;
        return audioMixerTable.ContainsKey(mask);
    }
    public static AudioMixerSnapshot? FindMixerSnapshot(this MusicMask mask)
    {
        if(!IsVaild(mask)) return null;
        return ResourcesUtils.FindAsset<AudioMixerSnapshot>(audioMixerTable[mask]);
    }
    public static bool TransitionTo(this MusicMask mask, float delayTime = 0, float timeToReach = 0)
    {
        var snapshot = mask.FindMixerSnapshot();
        if(snapshot == null) return false;
        GameManager.instance.AudioManager.ApplyMusicSnapshot(snapshot, delayTime, timeToReach);
        return true;
    }
}
