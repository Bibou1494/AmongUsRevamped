using AmongUs.Data;
using AmongUs.GameOptions;
using InnerNet;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
internal static class OnGameJoinedPatch
{
    public static bool AutoStartCheck;
    public static void Postfix(AmongUsClient __instance)
    {
        if (!AmongUsClient.Instance.AmHost || !Main.AutoStart.Value) return;

        LateTask.Tasks.Clear();

        new LateTask(() =>
        {
            AutoStartCheck = true;
        }, Options.WaitAutoStart.GetFloat(), "AutoStartTimer");
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
class OnPlayerJoinedPatch
{
    public static bool HasInvalidFriendCode(string friendcode)
    {
        if (string.IsNullOrEmpty(friendcode))
        {
            return true;
        }

        if (friendcode.Count(c => c == '#') != 1)
        {
            return true;
        }

        string pattern = @"[\W\d]";
        if (Regex.IsMatch(friendcode[..friendcode.IndexOf("#")], pattern))
        {
            return true;
        }

        return false;
    }

    static void Postfix([HarmonyArgument(0)] ClientData Client)
    {
        Logger.Info($" {Client.PlayerName} / {Client.FriendCode} / {Client.PlatformData.Platform}", "Joined The Game");

        if (AmongUsClient.Instance.AmHost)
        {
            BanManager.CheckBanPlayer(Client);


            if (HasInvalidFriendCode(Client.FriendCode) && Options.KickInvalidFriendCodes.GetBool())
            {
                if (!Options.TempBanInvalidFriendCodes.GetBool())
                {
                    AmongUsClient.Instance.KickPlayer(Client.Id, false);
                    Logger.Info($" {Client.PlayerName} Was kicked for having an invalid FriendCode", "KickInvalidFriendCode");
                    Logger.SendInGame($" {Client.PlayerName} Was kicked for having an invalid FriendCode");
                }
                else
                {
                    AmongUsClient.Instance.KickPlayer(Client.Id, true);
                    Logger.Info($" {Client.PlayerName} Was banned for having an invalid FriendCode", "BanInvalidFriendCode");
                    Logger.SendInGame($" {Client.PlayerName} Was banned for having an invalid FriendCode");
                }
            }
        }
    }
}