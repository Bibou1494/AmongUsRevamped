using AmongUs.Data;
using AmongUs.GameOptions;
using InnerNet;

namespace HNSRevamped;
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
class FixedUpdateInGamePatch
{
    public static void Postfix(PlayerControl __instance)
    {
        if (__instance == null || __instance.PlayerId == 255) return;

        if (Options.Gamemode.GetValue() == 1 && !Utils.isHideNSeek && Main.NormalOptions.KillCooldown != 0.01f)
        {
            Main.NormalOptions.KillCooldown = 0.01f;

            Main.NormalOptions.EmergencyCooldown = 0;
            Main.NormalOptions.DiscussionTime = 0;

            Main.NormalOptions.NumCommonTasks = 0;
            Main.NormalOptions.NumShortTasks = 1;
            Main.NormalOptions.NumLongTasks = 0;
        }

        if (Options.Gamemode.GetValue() < 1 && Main.NormalOptions.KillCooldown <= 0.01f)
        {
            Main.NormalOptions.KillCooldown = 25f;
        }

        if (AmongUsClient.Instance.AmHost)
        {
            if (__instance.Data.PlayerLevel != 0 && __instance.Data.PlayerLevel < Options.KickLowLevelPlayer.GetInt() && __instance.Data.ClientId != AmongUsClient.Instance.HostId)
            {
                if (!Options.TempBanLowLevelPlayer.GetBool()) 
                {
                    AmongUsClient.Instance.KickPlayer(__instance.Data.ClientId, false);
                    Logger.Info($" {__instance.Data.PlayerName} was kicked for being under level {Options.KickLowLevelPlayer.GetInt()}", "KickLowLevelPlayer");
                    Logger.SendInGame($" {__instance.Data.PlayerName} was kicked for being under level {Options.KickLowLevelPlayer.GetInt()}");
                }
                else
                {
                    AmongUsClient.Instance.KickPlayer(__instance.Data.ClientId, true);
                    Logger.Info($" {__instance.Data.PlayerName} was banned for being under level {Options.KickLowLevelPlayer.GetInt()} ", "BanLowLevelPlayer");
                    Logger.SendInGame($" {__instance.Data.PlayerName} was banned for being under level {Options.KickLowLevelPlayer.GetInt()}");
                }
            }
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
class ReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        if (target != null && Options.DisableReport.GetBool())
        {
            return false;
        }
        else return true;
    }
}