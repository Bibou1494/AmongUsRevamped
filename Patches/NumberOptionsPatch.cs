using AmongUs.GameOptions;

namespace HNSRevamped;

// https://github.com/astra1dev/AUnlocker/blob/main/src/OptionsPatches.cs

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
public static class NumberOption_Increase
{
    public static bool Prefix(NumberOption __instance)
    {
        if (Utils.isHideNSeek || !Utils.isHideNSeek && __instance.TitleText.text != "# Impostors" && __instance.TitleText.text != "Player Speed")
        {
            __instance.Value +=  __instance.Increment;
            __instance.UpdateValue();
            __instance.OnValueChanged.Invoke(__instance);
            __instance.AdjustButtonsActiveState();
            return false;
        }
        else return true;
    }
}

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
public static class NumberOption_Decrease
{
    public static bool Prefix(NumberOption __instance)
    {
        if (Utils.isHideNSeek || !Utils.isHideNSeek && __instance.TitleText.text != "# Impostors" && __instance.TitleText.text != "Player Speed")
        {
            __instance.Value -=  __instance.Increment;
            __instance.UpdateValue();
            __instance.OnValueChanged.Invoke(__instance);
            __instance.AdjustButtonsActiveState();
            return false;
        }
        else return true;
    }
}

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Initialize))]
public static class NumberOption_Initialize
{
    public static void Postfix(NumberOption __instance)
    {
        if (Utils.isHideNSeek || !Utils.isHideNSeek && __instance.TitleText.text != "# Impostors" && __instance.TitleText.text != "Player Speed")
        {
            __instance.ValidRange = new FloatRange(-999f, 999f);
        }

        if (Main.NoKcdMode.Value && !Utils.isHideNSeek)
        {
            Main.NormalOptions.KillCooldown = 0f;

            Main.NormalOptions.EmergencyCooldown = 0;
            Main.NormalOptions.DiscussionTime = 0;

            Main.NormalOptions.NumCommonTasks = 0;
            Main.NormalOptions.NumShortTasks = 1;
            Main.NormalOptions.NumLongTasks = 0;
        }
    }
}
