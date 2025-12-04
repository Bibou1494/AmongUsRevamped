using AmongUs.GameOptions;

namespace HNSRevamped;

public static class Utils
{
    public static bool isHideNSeek => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek;
}